using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Services;

public class ExameMedicoService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;
    
    public ExameMedicoService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<ExameMedico>> GetAllAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.ExameMedicos.ToListAsync();
    }
}