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

    
    public async Task<Utilizador> RegisterAsync(Utilizador utilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();
        if (utilizador.Password.Length > 8) // checks for pass quality, only size for now
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(utilizador.Password, workFactor: 12);

            utilizador.Password = hash;
            context.Utilizadores.Add(utilizador);
            context.SaveChangesAsync();
            return context.Utilizadores.FirstOrDefault(u => u.Username == utilizador.Username);
        }else
        {
            return null;
        }
    }
}