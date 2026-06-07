using BlazorProject.Data;
using BlazorProject.Data.Models;
using BlazorProject.Services;
using BlazorProject.UnitTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.UnitTests;

[TestFixture]
public class UtilizadorServiceTests
{
    private DbContextOptions<EiEngsofContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<EiEngsofContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task RegisterAsync_WithUniqueEmailAndUsername_HashesPasswordAndPersistsUser()
    {
        var options = CreateOptions();
        var factory = new TestDbContextFactory(options);
        var service = new UtilizadorService(factory);
        var utilizador = new Utilizador(
            nome: "Medico Exemplo",
            username: "medico.exemplo",
            password: "PasswordSegura9",
            telefone: "912345678",
            email: "medico@example.com",
            numCarteira: "OM12345");

        var result = await service.RegisterAsync(utilizador);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Password, Is.Not.EqualTo("PasswordSegura9"));
        Assert.That(BCrypt.Net.BCrypt.Verify("PasswordSegura9", result.Password), Is.True);

        await using var assertionContext = new TestEiEngsofContext(options);
        var persisted = await assertionContext.Utilizadores.SingleAsync();
        Assert.That(persisted.Email, Is.EqualTo("medico@example.com"));
        Assert.That(BCrypt.Net.BCrypt.Verify("PasswordSegura9", persisted.Password), Is.True);
    }

    [Test]
    public void RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Utilizadores.Add(new Utilizador(
                nome: "Existente",
                username: "existente",
                password: BCrypt.Net.BCrypt.HashPassword("OutraPassword9"),
                telefone: "919999999",
                email: "duplicado@example.com",
                numCarteira: "OM9"));
            seedContext.SaveChanges();
        }

        var service = new UtilizadorService(new TestDbContextFactory(options));
        var candidate = new Utilizador(
            nome: "Novo",
            username: "novo",
            password: "PasswordSegura9",
            telefone: "918888888",
            email: "duplicado@example.com",
            numCarteira: "OM10");

        var action = async () => await service.RegisterAsync(candidate);

        Assert.That(action, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Email já registado."));
    }

    [Test]
    public async Task LoginAsync_WithMatchingPassword_ReturnsUser()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Utilizadores.Add(new Utilizador(
                nome: "Medico",
                username: "medico",
                password: BCrypt.Net.BCrypt.HashPassword("PasswordSegura9"),
                telefone: "917777777",
                email: "login@example.com",
                numCarteira: "OM11"));
            seedContext.SaveChanges();
        }

        var service = new UtilizadorService(new TestDbContextFactory(options));

        var result = await service.LoginAsync("login@example.com", "PasswordSegura9");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("medico"));
    }

    [Test]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Utilizadores.Add(new Utilizador(
                nome: "Medico",
                username: "medico",
                password: BCrypt.Net.BCrypt.HashPassword("PasswordSegura9"),
                telefone: "916666666",
                email: "wrongpass@example.com",
                numCarteira: "OM12"));
            seedContext.SaveChanges();
        }

        var service = new UtilizadorService(new TestDbContextFactory(options));

        var result = await service.LoginAsync("wrongpass@example.com", "PasswordErrada9");

        Assert.That(result, Is.Null);
    }
}