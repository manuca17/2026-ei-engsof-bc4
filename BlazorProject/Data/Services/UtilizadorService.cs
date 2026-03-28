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

        var existing = await context.Utilizadores.FirstOrDefaultAsync(u => u.IdUtilizador == utilizador.IdUtilizador);

        if (existing is null)
        {
            throw new Exception("User not found");
        }

        existing.Nome = utilizador.Nome;
        existing.Email = utilizador.Email;
        existing.Telefone = utilizador.Telefone;
        existing.Especialidade = utilizador.Especialidade;
        existing.NumPorta = utilizador.NumPorta;
        existing.Rua = utilizador.Rua;
        existing.CodPostal = utilizador.CodPostal;
        existing.NumCarteira = utilizador.NumCarteira;

        if (!string.IsNullOrWhiteSpace(utilizador.Password))
        {
            var password = utilizador.Password;
            existing.Password = password.StartsWith("$2")
                ? password
                : BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        await context.SaveChangesAsync();
        return existing;
    }
}