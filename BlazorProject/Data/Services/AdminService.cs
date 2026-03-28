using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Services;

public class AdminService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;

    public AdminService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }

    // ── User Management ────────────────────────────────────────────────────────

    public async Task<List<AdminMedicoItem>> GetAllMedicosAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Utilizadores
            .AsNoTracking()
            .Where(u => !u.IsAdmin)
            .OrderBy(u => u.Nome)
            .Select(u => new AdminMedicoItem
            {
                Id = u.IdUtilizador,
                Nome = u.Nome,
                Email = u.Email,
                Especialidade = u.Especialidade,
                Telefone = u.Telefone,
                NumCarteira = u.NumCarteira,
                Username = u.Username
            })
            .ToListAsync();
    }

    public async Task<(bool Ok, string Message)> CreateMedicoAsync(CreateMedicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Nome, email e senha são obrigatórios.");
        }

        await using var context = await _factory.CreateDbContextAsync();

        if (await context.Utilizadores.AnyAsync(u => u.Email == request.Email.Trim()))
        {
            return (false, "Já existe um utilizador com este email.");
        }

        var username = request.Email.Split('@')[0];
        if (await context.Utilizadores.AnyAsync(u => u.Username == username))
        {
            username = username + "_" + DateTime.Now.Ticks.ToString()[^4..];
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var medico = new Utilizador(
            nome: request.Nome.Trim(),
            username: username,
            password: hash,
            telefone: string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim(),
            email: request.Email.Trim(),
            numCarteira: string.IsNullOrWhiteSpace(request.NumCarteira) ? null : request.NumCarteira.Trim())
        {
            Especialidade = string.IsNullOrWhiteSpace(request.Especialidade) ? null : request.Especialidade.Trim(),
            IsAdmin = false,
            IsManager = false
        };

        context.Utilizadores.Add(medico);
        await context.SaveChangesAsync();

        return (true, $"Médico {medico.Nome} criado com sucesso.");
    }

    public async Task<(bool Ok, string Message)> DeleteMedicoAsync(int idMedico)
    {
        await using var context = await _factory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var medico = await context.Utilizadores.FindAsync(idMedico);
            if (medico is null) return (false, "Médico não encontrado.");
            if (medico.IsAdmin) return (false, "Não é possível eliminar um administrador.");

            // IDs das consultas onde este utilizador é criador
            var consultaIds = await context.UtilizadorConsulta
                .Where(uc => uc.IdUtilizador == idMedico && uc.IsCriador)
                .Select(uc => uc.IdConsulta)
                .ToListAsync();

            // Apagar anotações (das consultas do médico + feitas por ele)
            await context.Anotacaos
                .Where(a => (a.IdConsulta.HasValue && consultaIds.Contains(a.IdConsulta.Value)) || a.IdUtilizador == idMedico)
                .ExecuteDeleteAsync();

            // Apagar ligações exame-consulta
            await context.ExameMedicoConsulta
                .Where(e => consultaIds.Contains(e.IdConsulta))
                .ExecuteDeleteAsync();

            // Apagar exames do médico
            await context.ExameMedicos
                .Where(e => e.IdUtilizador == idMedico)
                .ExecuteDeleteAsync();

            // Apagar estados das consultas
            await context.Estados
                .Where(e => consultaIds.Contains(e.IdConsulta))
                .ExecuteDeleteAsync();

            // Apagar ligações utilizador-consulta
            await context.UtilizadorConsulta
                .Where(uc => consultaIds.Contains(uc.IdConsulta) || uc.IdUtilizador == idMedico)
                .ExecuteDeleteAsync();

            // Apagar consultas
            await context.Consulta
                .Where(c => consultaIds.Contains(c.IdConsulta))
                .ExecuteDeleteAsync();

            // Apagar pacientes do médico
            await context.Pacientes
                .Where(p => p.IdUtilizador == idMedico)
                .ExecuteDeleteAsync();

            // Apagar tipos de consulta do médico
            await context.TipoConsulta
                .Where(tc => tc.IdUtilizador == idMedico)
                .ExecuteDeleteAsync();

            // Apagar utilizador
            context.Utilizadores.Remove(medico);
            await context.SaveChangesAsync();

            await tx.CommitAsync();
            return (true, $"Médico {medico.Nome} eliminado com sucesso.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Erro ao eliminar médico: {ex.Message}");
        }
    }

    // ── Global Records ─────────────────────────────────────────────────────────

    public async Task<GlobalStatsItem> GetGlobalStatsAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        var totalConsultas = await context.Consulta.CountAsync();
        var totalPacientes = await context.Pacientes.CountAsync();
        var totalMedicos = await context.Utilizadores.CountAsync(u => !u.IsAdmin);
        var totalExames = await context.ExameMedicoConsulta.CountAsync();
        var totalAnotacoes = await context.Anotacaos.CountAsync();

        var encerradas = await context.Consulta
            .AsNoTracking()
            .Where(c => c.Estados.OrderByDescending(e => e.DhRegisto)
                .Select(e => e.EstadoTo)
                .FirstOrDefault()!.ToLower().Contains("encerr"))
            .Select(c => new { c.ValorTotal, c.ValorHora, c.DhInicio, c.DhFim })
            .ToListAsync();

        decimal totalReceita = 0m;
        foreach (var c in encerradas)
        {
            if (c.ValorHora.HasValue && c.ValorHora.Value > 0 && c.DhFim.HasValue)
            {
                var hours = (decimal)(c.DhFim.Value - c.DhInicio).TotalHours;
                totalReceita += c.ValorHora.Value * Math.Max(0, hours);
            }
            else if (c.ValorTotal.HasValue)
            {
                totalReceita += c.ValorTotal.Value;
            }
        }

        return new GlobalStatsItem
        {
            TotalConsultas = totalConsultas,
            TotalPacientes = totalPacientes,
            TotalMedicos = totalMedicos,
            TotalExames = totalExames,
            TotalAnotacoes = totalAnotacoes,
            TotalReceita = totalReceita
        };
    }

    public async Task<List<AdminConsultationItem>> GetAllConsultationsAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Consulta
            .AsNoTracking()
            .OrderByDescending(c => c.DhInicio)
            .Select(c => new AdminConsultationItem
            {
                Id = c.IdConsulta,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Sem paciente",
                DoctorName = c.UtilizadorConsulta
                    .Where(uc => uc.IsCriador)
                    .Select(uc => uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Nome : "Desconhecido")
                    .FirstOrDefault() ?? "Desconhecido",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                Status = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault() ?? "Agendada",
                DhInicio = c.DhInicio,
                DhFim = c.DhFim,
                ValorTotal = c.ValorTotal,
                ValorHora = c.ValorHora,
                IsHourly = c.ValorHora.HasValue && c.ValorHora.Value > 0,
                ExamsCount = c.ExamesDaConsulta.Count(),
                AnnotationsCount = c.Anotacaos.Count()
            })
            .ToListAsync();
    }

    public async Task<List<AdminPatientItem>> GetAllPatientsAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Pacientes
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .Select(p => new AdminPatientItem
            {
                Id = p.IdPaciente,
                Nome = p.Nome,
                Email = p.Email,
                Telefone = p.Telefone,
                DtNasc = p.DtNasc,
                Rua = p.Rua,
                DoctorName = p.IdUtilizadorNavigation != null ? p.IdUtilizadorNavigation.Nome : "Desconhecido"
            })
            .ToListAsync();
    }

    // ── DTOs ───────────────────────────────────────────────────────────────────

    public sealed class AdminMedicoItem
    {
        public int Id { get; init; }
        public string Nome { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Especialidade { get; init; }
        public string? Telefone { get; init; }
        public string? NumCarteira { get; init; }
        public string Username { get; init; } = string.Empty;
    }

    public sealed class CreateMedicoRequest
    {
        public string Nome { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string? Especialidade { get; init; }
        public string? Telefone { get; init; }
        public string? NumCarteira { get; init; }
    }

    public sealed class GlobalStatsItem
    {
        public int TotalConsultas { get; init; }
        public int TotalPacientes { get; init; }
        public int TotalMedicos { get; init; }
        public int TotalExames { get; init; }
        public int TotalAnotacoes { get; init; }
        public decimal TotalReceita { get; init; }
    }

    public sealed class AdminConsultationItem
    {
        public int Id { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime DhInicio { get; init; }
        public DateTime? DhFim { get; init; }
        public decimal? ValorTotal { get; init; }
        public decimal? ValorHora { get; init; }
        public bool IsHourly { get; init; }
        public int ExamsCount { get; init; }
        public int AnnotationsCount { get; init; }

        public decimal CalculatedPrice
        {
            get
            {
                if (IsHourly && DhFim.HasValue)
                {
                    var hours = (decimal)(DhFim.Value - DhInicio).TotalHours;
                    return Math.Round((ValorHora ?? 0m) * Math.Max(0, hours), 2);
                }
                return ValorTotal ?? 0m;
            }
        }

        public string StatusNormalized
        {
            get
            {
                var s = Status.Trim().ToLowerInvariant();
                if (s.Contains("encerr")) return "Encerrada";
                if (s.Contains("andamento") || s.Contains("curso")) return "Em Andamento";
                return "Agendada";
            }
        }
    }

    public sealed class AdminPatientItem
    {
        public int Id { get; init; }
        public string Nome { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Telefone { get; init; }
        public DateOnly? DtNasc { get; init; }
        public string? Rua { get; init; }
        public string DoctorName { get; init; } = string.Empty;
    }
}
