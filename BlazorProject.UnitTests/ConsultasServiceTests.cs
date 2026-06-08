using BlazorProject.Data;
using BlazorProject.Data.Models;
using BlazorProject.Services;
using BlazorProject.UnitTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.UnitTests;

[TestFixture]
public class ConsultasServiceTests
{
    private DbContextOptions<EiEngsofContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<EiEngsofContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task GetPatientsForDoctorAsync_WithInvalidDoctorId_ReturnsEmptyList()
    {
        var service = new ConsultasService(new TestDbContextFactory(CreateOptions()));

        var result = await service.GetPatientsForDoctorAsync(0);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPatientsForDoctorAsync_ReturnsDistinctOwnAndAcceptedPatientsOrderedByName()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Pacientes.AddRange(
                new Paciente { IdPaciente = 1, IdUtilizador = 5, Nome = "Marta" },
                new Paciente { IdPaciente = 2, IdUtilizador = 9, Nome = "Ana" },
                new Paciente { IdPaciente = 3, IdUtilizador = 9, Nome = "Tiago" });

            seedContext.Consulta.AddRange(
                new Consulta { IdConsulta = 200, IdPaciente = 2, DhInicio = new DateTime(2026, 5, 1, 9, 0, 0) },
                new Consulta { IdConsulta = 201, IdPaciente = 2, DhInicio = new DateTime(2026, 5, 2, 9, 0, 0) },
                new Consulta { IdConsulta = 202, IdPaciente = 3, DhInicio = new DateTime(2026, 5, 3, 9, 0, 0) });

            seedContext.UtilizadorConsulta.AddRange(
                new UtilizadorConsulta { IdUtilizador = 5, IdConsulta = 200, IsCriador = false, ConviteAceite = true },
                new UtilizadorConsulta { IdUtilizador = 5, IdConsulta = 201, IsCriador = true, ConviteAceite = false },
                new UtilizadorConsulta { IdUtilizador = 5, IdConsulta = 202, IsCriador = false, ConviteAceite = false });

            seedContext.SaveChanges();
        }

        var service = new ConsultasService(new TestDbContextFactory(options));

        var result = await service.GetPatientsForDoctorAsync(5);

        Assert.That(result.Select(p => p.Name), Is.EqualTo(new[] { "Ana", "Marta" }));
    }

    [Test]
    public async Task GetByIdForDoctorAsync_WithoutPermission_ReturnsNull()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Pacientes.Add(new Paciente { IdPaciente = 10, Nome = "Paciente" });
            seedContext.Consulta.Add(new Consulta { IdConsulta = 300, IdPaciente = 10, DhInicio = new DateTime(2026, 5, 10, 10, 0, 0) });
            seedContext.UtilizadorConsulta.Add(new UtilizadorConsulta
            {
                IdUtilizador = 8,
                IdConsulta = 300,
                IsCriador = false,
                ConviteAceite = false
            });
            seedContext.SaveChanges();
        }

        var service = new ConsultasService(new TestDbContextFactory(options));

        var result = await service.GetByIdForDoctorAsync(8, 300);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdForDoctorAsync_WithAcceptedAccess_ReturnsProjectedConsultationSummary()
    {
        var options = CreateOptions();

        using (var seedContext = new TestEiEngsofContext(options))
        {
            seedContext.Pacientes.Add(new Paciente { IdPaciente = 20, Nome = "Helena" });
            seedContext.Consulta.Add(new Consulta
            {
                IdConsulta = 400,
                IdPaciente = 20,
                DhInicio = new DateTime(2026, 5, 20, 14, 0, 0),
                ValorHora = 60m,
                ValorTotal = 120m
            });
            seedContext.Estados.Add(new Estado
            {
                IdEstado = 1,
                IdConsulta = 400,
                EstadoTo = "Em andamento",
                Comentario = "Consulta iniciada",
                DhRegisto = new DateTime(2026, 5, 20, 14, 5, 0)
            });
            seedContext.UtilizadorConsulta.AddRange(
                new UtilizadorConsulta { IdUtilizador = 15, IdConsulta = 400, IsCriador = true, ConviteAceite = false },
                new UtilizadorConsulta { IdUtilizador = 16, IdConsulta = 400, IsCriador = false, ConviteAceite = true });
            seedContext.SaveChanges();
        }

        var service = new ConsultasService(new TestDbContextFactory(options));

        var result = await service.GetByIdForDoctorAsync(16, 400);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.PatientName, Is.EqualTo("Helena"));
        Assert.That(result.Description, Is.EqualTo("Consulta iniciada"));
        Assert.That(result.Status, Is.EqualTo(ConsultasService.ConsultationStatus.EmAndamento));
        Assert.That(result.ChargingType, Is.EqualTo(ConsultasService.ChargingType.PorHora));
        Assert.That(result.InvitesCount, Is.EqualTo(1));
    }
}