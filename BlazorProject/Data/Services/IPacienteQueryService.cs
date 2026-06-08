using BlazorProject.Data.Models;

namespace BlazorProject.Data.Services;

public interface IPacienteQueryService
{
    Task<List<Paciente>> GetByDoctorAsync(int idUtilizador);
}