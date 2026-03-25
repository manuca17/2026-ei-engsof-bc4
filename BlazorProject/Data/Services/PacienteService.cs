using Microsoft.EntityFrameworkCore;
using BlazorProject.Data.Models; // Ensure this matches your context namespace

namespace BlazorProject.Data.Services;

public class PacienteService
{
    private readonly IDbContextFactory<EiEngsofContext> _contextFactory;

    public PacienteService(IDbContextFactory<EiEngsofContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Retrieves all patients from the database.
    /// </summary>
    public async Task<List<Paciente>> GetPacientesAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Pacientes
            .Include(p => p.CodPostalNavigation) // Include related data if needed
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a single patient by their ID.
    /// </summary>
    public async Task<Paciente?> GetPacienteByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Pacientes
            .Include(p => p.CodPostalNavigation)
            .FirstOrDefaultAsync(p => p.IdPaciente == id);
    }
    
    public async Task<List<Paciente>> GetByDoctorAsync(int idUtilizador)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

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


    /// <summary>
    /// Adds a new patient or updates an existing one.
    /// </summary>
    public async Task SavePacienteAsync(Paciente paciente)
    {
        using var context = _contextFactory.CreateDbContext();
        
        if (paciente.IdPaciente == 0)
            context.Pacientes.Add(paciente);
        else
            context.Pacientes.Update(paciente);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a patient by ID.
    /// </summary>
    public async Task DeletePacienteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var paciente = await context.Pacientes.FindAsync(id);
        if (paciente != null)
        {
            context.Pacientes.Remove(paciente);
            await context.SaveChangesAsync();
        }
    }
}