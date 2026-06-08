using BlazorProject.Data;
using BlazorProject.Data.Models;
using BlazorProject.Data.Services;
using BlazorProject.UnitTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.UnitTests;

[TestFixture]
public class PacienteServiceTests
{
    private DbContextOptions<EiEngsofContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<EiEngsofContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task SavePacienteAsync_NewPatientWithoutOwner_AssignsCreatorUserId()
    {
        var options = CreateOptions();
        var service = new PacienteService(new TestDbContextFactory(options));
        var paciente = new Paciente
        {
            Nome = "Carlos Mendes",
            Telefone = "912345678",
            Email = "carlos@example.com",
            DtNasc = DateOnly.FromDateTime(DateTime.Today.AddYears(-30))
        };

        await service.SavePacienteAsync(paciente, creatorUserId: 42);

        await using var assertionContext = new TestEiEngsofContext(options);
        var persisted = await assertionContext.Pacientes.SingleAsync();
        Assert.That(persisted.IdUtilizador, Is.EqualTo(42));
        Assert.That(persisted.Nome, Is.EqualTo("Carlos Mendes"));
    }

    [Test]
    public void SavePacienteAsync_WithFutureBirthDate_ThrowsArgumentException()
    {
        var options = CreateOptions();
        var service = new PacienteService(new TestDbContextFactory(options));
        var paciente = new Paciente
        {
            Nome = "Paciente Futuro",
            Telefone = "919999999",
            DtNasc = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        var action = async () => await service.SavePacienteAsync(paciente, creatorUserId: 10);

        Assert.That(action, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task GetByDoctorAsync_ReturnsOwnAndAcceptedConsultationPatientsOrderedByName()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Pacientes.AddRange(
                new Paciente { IdPaciente = 1, IdUtilizador = 7, Nome = "Zelda" },
                new Paciente { IdPaciente = 2, IdUtilizador = 9, Nome = "Ana" },
                new Paciente { IdPaciente = 3, IdUtilizador = 9, Nome = "Bruno" });

            seedContext.Consulta.AddRange(
                new Consulta { IdConsulta = 100, IdPaciente = 2, DhInicio = DateTime.UtcNow },
                new Consulta { IdConsulta = 101, IdPaciente = 3, DhInicio = DateTime.UtcNow });

            seedContext.UtilizadorConsulta.AddRange(
                new UtilizadorConsulta { IdUtilizador = 7, IdConsulta = 100, IsCriador = false, ConviteAceite = true },
                new UtilizadorConsulta { IdUtilizador = 7, IdConsulta = 101, IsCriador = false, ConviteAceite = false });

            seedContext.SaveChanges();
        }

        var service = new PacienteService(new TestDbContextFactory(options));

        var result = await service.GetByDoctorAsync(7);

        Assert.That(result.Select(p => p.Nome), Is.EqualTo(new[] { "Ana", "Zelda" }));
    }
}