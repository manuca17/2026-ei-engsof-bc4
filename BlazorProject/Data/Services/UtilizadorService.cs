namespace BlazorProject.Services;
using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

public class UtilizadorService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;

    public UtilizadorService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }
    
    public async Task<Utilizador?> LoginAsync(string username, string password)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Utilizadors
            .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
    }
}