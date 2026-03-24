using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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
    
    public async Task<Utilizador?> LoginAsync(string email, string password)
    {
        await using var context = await _factory.CreateDbContextAsync();
        
        var Utilizador = context.Utilizadores.FirstOrDefaultAsync(u => u.Email == email);

        if (Utilizador.Result == null)
        {
            return null;
        }
        if (BCrypt.Net.BCrypt.Verify(password, Utilizador.Result.Password))
        {
            return Utilizador.Result;
        }

        return null;
    }
    
    public async Task<Utilizador?> RegisterAsync(Utilizador utilizador)
    {
        if (string.IsNullOrWhiteSpace(utilizador.Password) || utilizador.Password.Length <= 8)
        {
            return null; 
        }

        await using var context = await _factory.CreateDbContextAsync();
        
        string hash = BCrypt.Net.BCrypt.HashPassword(utilizador.Password, workFactor: 12);
        utilizador.Password = hash;

        context.Utilizadores.Add(utilizador);

        await context.SaveChangesAsync();

        return utilizador;
    }

    public async Task<Utilizador> UpdateAsync(Utilizador utilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();


        var exists = await context.Utilizadores.AnyAsync(u => u.IdUtilizador == utilizador.IdUtilizador);
    
        if (!exists) 
        {
            throw new Exception("User not found");
        }

        utilizador.DataHoraAtualizacao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        context.Utilizadores.Update(utilizador);

        await context.SaveChangesAsync();
        return null;
    }
}