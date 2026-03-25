using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Services;

public class ConsultasService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;

    public ConsultasService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<PatientOptionItem>> GetPatientsForDoctorAsync(int idUtilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var patientIds = context.UtilizadorConsulta
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation.IdPaciente)
            .Where(idPaciente => idPaciente.HasValue)
            .Select(idPaciente => idPaciente!.Value)
            .Distinct();

        return await context.Pacientes
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.IdPaciente))
            .OrderBy(p => p.Nome)
            .Select(p => new PatientOptionItem
            {
                Id = p.IdPaciente,
                Name = p.Nome
            })
            .ToListAsync();
    }

    public async Task<List<ConsultationListItem>> GetForDoctorAsync(int idUtilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation)
            .OrderByDescending(c => c.DhInicio)
            .Select(c => new ConsultationListItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                Status = MapStatus(c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault()),
                ChargingType = c.ValorHora.HasValue && c.ValorHora.Value > 0
                    ? ChargingType.PorHora
                    : ChargingType.Fixo,
                FixedPrice = c.ValorTotal,
                HourlyPrice = c.ValorHora,
                StartAt = c.DhInicio,
                ExamsCount = c.ExamesDaConsulta.Count(),
                NotesCount = c.Anotacaos.Count(),
                InvitesCount = c.UtilizadorConsulta.Count(uc => !uc.IsCriador)
            })
            .ToListAsync();
    }

    public async Task<ConsultationListItem?> GetByIdForDoctorAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite) && link.IdConsulta == idConsulta)
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new ConsultationListItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                Status = MapStatus(c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault()),
                ChargingType = c.ValorHora.HasValue && c.ValorHora.Value > 0
                    ? ChargingType.PorHora
                    : ChargingType.Fixo,
                FixedPrice = c.ValorTotal,
                HourlyPrice = c.ValorHora,
                StartAt = c.DhInicio,
                ExamsCount = c.ExamesDaConsulta.Count(),
                NotesCount = c.Anotacaos.Count(),
                InvitesCount = c.UtilizadorConsulta.Count(uc => !uc.IsCriador)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ConsultationDetailItem?> GetDetailForDoctorAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var consulta = await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite) && link.IdConsulta == idConsulta)
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new
            {
                Consulta = c,
                LatestEstado = c.Estados.OrderByDescending(e => e.DhRegisto).FirstOrDefault(),
                Notes = c.Anotacaos
                    .OrderByDescending(n => n.DhCriacao)
                    .Select(n => new DetailAnnotationItem
                    {
                        Id = n.IdAnotacao,
                        Text = n.Descricao ?? string.Empty,
                        CreatedAt = n.DhCriacao,
                        UserId = n.IdUtilizador,
                        UserName = n.IdUtilizadorNavigation != null ? n.IdUtilizadorNavigation.Nome : "Utilizador"
                    })
                    .ToList(),
                    CurrentExams = c.ExamesDaConsulta.Count == 0
                        ? new List<DetailExamItem>()
                        : c.ExamesDaConsulta.Select(e => new DetailExamItem
                        {
                            Id = e.IdExameMedicoNavigation.IdExameMedico,
                            Name = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                            Type = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                            Date = e.IdExameMedicoNavigation.DhExame,
                            Description = null,
                            Results = e.IdExameMedicoNavigation.Resultado
                        }).ToList()
            })
            .FirstOrDefaultAsync();

        if (consulta is null)
        {
            return null;
        }

        List<DetailExamItem> availableExams = [];
        if (consulta.Consulta.IdPaciente.HasValue)
        {
            var currentExamIds = consulta.CurrentExams.Select(e => e.Id).ToHashSet();

            availableExams = await context.ExameMedicoConsulta
                .AsNoTracking()
                .Where(emc => emc.IdConsultaNavigation!.IdPaciente == consulta.Consulta.IdPaciente
                              && !currentExamIds.Contains(emc.IdExameMedico))
                .Select(emc => new DetailExamItem
                {
                    Id = emc.IdExameMedicoNavigation!.IdExameMedico,
                    Name = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Type = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Date = emc.IdExameMedicoNavigation.DhExame,
                    Description = null,
                    Results = emc.IdExameMedicoNavigation.Resultado
                })
                .Distinct()
                .ToListAsync();
        }

        return new ConsultationDetailItem
        {
            Id = consulta.Consulta.IdConsulta,
            PatientId = consulta.Consulta.IdPaciente,
            PatientName = consulta.Consulta.IdPacienteNavigation != null ? consulta.Consulta.IdPacienteNavigation.Nome : "Paciente sem nome",
            Description = consulta.LatestEstado?.Comentario ?? "Consulta médica",
            Status = MapStatus(consulta.LatestEstado?.EstadoTo),
            ChargingType = consulta.Consulta.ValorHora.HasValue && consulta.Consulta.ValorHora.Value > 0 ? ChargingType.PorHora : ChargingType.Fixo,
            FixedPrice = consulta.Consulta.ValorTotal,
            HourlyPrice = consulta.Consulta.ValorHora,
            StartAt = consulta.Consulta.DhInicio,
            EndAt = consulta.Consulta.DhFim,
            Exams = consulta.CurrentExams,
            AvailableExams = availableExams,
            Annotations = consulta.Notes
        };
    }

    public async Task<bool> StartConsultationAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return false;
        }

        context.Estados.Add(new Estado
        {
            IdConsulta = idConsulta,
            EstadoTo = "Em andamento",
            Comentario = "Consulta iniciada",
            DhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EndConsultationAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return false;
        }

        var consulta = await context.Consulta.FirstOrDefaultAsync(c => c.IdConsulta == idConsulta);
        if (consulta is null)
        {
            return false;
        }

        consulta.DhFim = DateTime.Now;
        context.Estados.Add(new Estado
        {
            IdConsulta = idConsulta,
            EstadoTo = "Encerrada",
            Comentario = "Consulta encerrada",
            DhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Ok, string Message)> AddNewExamAsync(int idUtilizador, int idConsulta, DateTime date, string type, string? results)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var exame = new ExameMedico
        {
            DhExame = date,
            Tipo = type,
            Resultado = results,
            IdUtilizador = idUtilizador
        };

        context.ExameMedicos.Add(exame);
        await context.SaveChangesAsync();

        context.ExameMedicoConsulta.Add(new ExameMedicoConsulta
        {
            IdExameMedico = exame.IdExameMedico,
            IdConsulta = idConsulta,
            IdUtilzador = idUtilizador,
            dhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Exame adicionado com sucesso");
    }

    public async Task<(bool Ok, string Message)> AssociateExamAsync(int idUtilizador, int idConsulta, int idExame)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var alreadyLinked = await context.ExameMedicoConsulta.AnyAsync(e =>
            e.IdConsulta == idConsulta && e.IdExameMedico == idExame);
        if (alreadyLinked)
        {
            return (false, "Este exame já está associado à consulta.");
        }

        var examExists = await context.ExameMedicos.AnyAsync(e => e.IdExameMedico == idExame);
        if (!examExists)
        {
            return (false, "Exame não encontrado.");
        }

        context.ExameMedicoConsulta.Add(new ExameMedicoConsulta
        {
            IdExameMedico = idExame,
            IdConsulta = idConsulta,
            IdUtilzador = idUtilizador,
            dhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Exame associado com sucesso");
    }

    public async Task<(bool Ok, string Message)> AddAnnotationAsync(int idUtilizador, int idConsulta, string text)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        context.Anotacaos.Add(new Anotacao
        {
            IdConsulta = idConsulta,
            IdUtilizador = idUtilizador,
            Descricao = text,
            DhCriacao = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Anotação adicionada com sucesso.");
    }

    public async Task<(bool Ok, string? ErrorMessage, int? IdConsulta)> CreateForDoctorAsync(CreateConsultaRequest request)
    {
        await using var context = await _factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var consulta = new Consulta
            {
                DhInicio = DateTime.Now,
                IdPaciente = request.IdPaciente,
                ValorTotal = request.ChargingType == ChargingType.Fixo ? request.FixedPrice : null,
                ValorHora = request.ChargingType == ChargingType.PorHora ? request.HourlyPrice : null
            };

            context.Consulta.Add(consulta);
            await context.SaveChangesAsync();

            context.UtilizadorConsulta.Add(new UtilizadorConsulta
            {
                IdUtilizador = request.IdUtilizador,
                IdConsulta = consulta.IdConsulta,
                IsCriador = true,
                ConviteAceite = true
            });

            context.Estados.Add(new Estado
            {
                IdConsulta = consulta.IdConsulta,
                EstadoTo = "Agendada",
                Comentario = request.Description,
                DhRegisto = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null, consulta.IdConsulta);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message, null);
        }
    }

    private static ConsultationStatus MapStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Contains("encerr"))
        {
            return ConsultationStatus.Encerrada;
        }

        if (normalized.Contains("andamento") || normalized.Contains("curso"))
        {
            return ConsultationStatus.EmAndamento;
        }

        return ConsultationStatus.Agendada;
    }

    public enum ConsultationStatus
    {
        Agendada,
        EmAndamento,
        Encerrada
    }

    public enum ChargingType
    {
        Fixo,
        PorHora
    }

    public sealed class PatientOptionItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public sealed class ConsultationListItem
    {
        public int Id { get; init; }
        public int? PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ConsultationStatus Status { get; init; }
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
        public DateTime? StartAt { get; init; }
        public int ExamsCount { get; init; }
        public int NotesCount { get; init; }
        public int InvitesCount { get; init; }
    }

    public sealed class CreateConsultaRequest
    {
        public int IdUtilizador { get; init; }
        public int IdPaciente { get; init; }
        public string Description { get; init; } = string.Empty;
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
    }

    public sealed class ConsultationDetailItem
    {
        public int Id { get; init; }
        public int? PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ConsultationStatus Status { get; init; }
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
        public DateTime? StartAt { get; init; }
        public DateTime? EndAt { get; init; }
        public List<DetailExamItem> Exams { get; init; } = [];
        public List<DetailExamItem> AvailableExams { get; init; } = [];
        public List<DetailAnnotationItem> Annotations { get; init; } = [];
    }

    public sealed class DetailExamItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public string? Description { get; init; }
        public string? Results { get; init; }
    }

    public sealed class DetailAnnotationItem
    {
        public int Id { get; init; }
        public string Text { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public int? UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
    }
}
