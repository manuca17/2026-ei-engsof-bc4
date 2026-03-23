using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Services;

public class PacienteService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;

    public PacienteService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Paciente>> GetAllAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Pacientes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Paciente?> GetByIdAsync(int idPaciente)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);
    }

    public async Task<List<Paciente>> GetByDoctorAsync(int idUtilizador)
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
            .ToListAsync();
    }

    public async Task<(bool Ok, string? ErrorMessage, Paciente? Paciente)> CreateForDoctorAsync(CreatePacienteRequest request)
    {
        await using var context = await _factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var novoPaciente = new Paciente
            {
                Nome = request.Nome,
                DtNasc = request.DtNasc,
                Telefone = request.Telefone,
                Email = request.Email,
                Rua = request.Rua,
                NumPorta = request.NumPorta,
                CodPostal = request.CodPostal,
                Nif = request.Nif
            };

            context.Pacientes.Add(novoPaciente);
            await context.SaveChangesAsync();

            var consulta = new Consulta
            {
                DhInicio = DateTime.Now,
                IdPaciente = novoPaciente.IdPaciente
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

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null, novoPaciente);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message, null);
        }
    }

    public sealed class CreatePacienteRequest
    {
        public int IdUtilizador { get; init; }
        public string Nome { get; init; } = string.Empty;
        public DateOnly? DtNasc { get; init; }
        public string? Telefone { get; init; }
        public string? Email { get; init; }
        public string? Rua { get; init; }
        public string? NumPorta { get; init; }
        public string? CodPostal { get; init; }
        public string? Nif { get; init; }
    }
}
