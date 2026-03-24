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
        
        var utilizador = await context.Utilizadores.FirstOrDefaultAsync(u => u.Email == email);

        if (utilizador == null) return null;

        if (BCrypt.Net.BCrypt.Verify(password, utilizador.Password))
        {
            return utilizador;
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

        bool usernameExists = await context.Utilizadores.AnyAsync(u => u.Username == utilizador.Username);
        if (usernameExists) throw new InvalidOperationException("Username já existe.");

        bool emailExists = await context.Utilizadores.AnyAsync(u => u.Email == utilizador.Email);
        if (emailExists) throw new InvalidOperationException("Email já registado.");

        string hash = BCrypt.Net.BCrypt.HashPassword(utilizador.Password, workFactor: 12);
        utilizador.Password = hash;

        context.Utilizadores.Add(utilizador);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[REGISTER] SaveChanges falhou: {ex.Message}");
            Console.WriteLine($"[REGISTER] Inner: {ex.InnerException?.Message}");
            Console.WriteLine($"[REGISTER] Inner2: {ex.InnerException?.InnerException?.Message}");
            Console.ResetColor();
            throw;
        }

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

        context.Utilizadores.Update(utilizador);

        await context.SaveChangesAsync();
        return null;
    }
}