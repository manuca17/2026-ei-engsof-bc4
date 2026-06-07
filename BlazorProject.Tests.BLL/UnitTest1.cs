using System;
using System.Threading.Tasks;
using BlazorProject.Data;
using BlazorProject.Data.Models;
using BlazorProject.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace BlazorProject.Tests.BLL;

[TestFixture]
public class ConsultasServiceTests
{
    private SqliteConnection _connection;
    private DbContextOptions<EiEngsofContext> _contextOptions;
    private IDbContextFactory<EiEngsofContext> _contextFactory;
    private ConsultasService _service;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        _contextOptions = new DbContextOptionsBuilder<EiEngsofContext>()
            .UseSqlite(_connection)
            .Options;

        using (var context = new EiEngsofContext(_contextOptions))
        {
            await context.Database.EnsureCreatedAsync();
        }

        _contextFactory = new PooledDbContextFactory<EiEngsofContext>(_contextOptions);
        _service = new ConsultasService(_contextFactory);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }

    // TEST 1: Early exit boundary check
    [Test]
    public async Task GetPatientsForDoctorAsync_ShouldReturnEmpty_WhenIdIsInvalid()
    {
        var result = await _service.GetPatientsForDoctorAsync(0);
        Assert.That(result, Is.Empty);
    }

    // TEST 2: Complex LINQ and business rules filtering
    [Test]
    public async Task GetPatientsForDoctorAsync_ShouldOnlyReturnOwnedOrAcceptedInvitePatients()
    {
        using (var context = new EiEngsofContext(_contextOptions))
        {
            var doctorId = 1;
            
            context.Pacientes.Add(new Paciente { IdPaciente = 1, IdUtilizador = doctorId, Nome = "Bernardo" });
            
            context.Pacientes.Add(new Paciente { IdPaciente = 2, IdUtilizador = 9, Nome = "Antonio" });
            context.Consulta.Add(new Consulta { IdConsulta = 10, IdPaciente = 2 });
            context.UtilizadorConsulta.Add(new UtilizadorConsulta { IdUtilizador = doctorId, IdConsulta = 10, IsCriador = false, ConviteAceite = true });

            context.Pacientes.Add(new Paciente { IdPaciente = 3, IdUtilizador = 9, Nome = "Carlos" });
            context.Consulta.Add(new Consulta { IdConsulta = 11, IdPaciente = 3 });
            context.UtilizadorConsulta.Add(new UtilizadorConsulta { IdUtilizador = doctorId, IdConsulta = 11, IsCriador = false, ConviteAceite = false });

            await context.SaveChangesAsync();
        }

        var result = await _service.GetPatientsForDoctorAsync(1);

        Assert.Multiple(() =>
        {
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Antonio"), "Should be ordered alphabetically");
            Assert.That(result[1].Name, Is.EqualTo("Bernardo"));
        });
    }

    [Test]
    public async Task SendInviteAsync_ShouldFail_WhenUserInvitesThemselves()
    {
        using (var context = new EiEngsofContext(_contextOptions))
        {
            var medico = new Utilizador("Dr. Silva", "dsilva", "pass123", "912345678", "medico@teste.com", "12345")
            {
                IdUtilizador = 1
            };

            context.Utilizadores.Add(medico);
            context.UtilizadorConsulta.Add(new UtilizadorConsulta { IdUtilizador = 1, IdConsulta = 50, IsCriador = true });
            await context.SaveChangesAsync();
        }

        var (ok, message) = await _service.SendInviteAsync(1, 50, "medico@teste.com");

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.False);
            Assert.That(message, Is.EqualTo("Não pode convidar-se a si mesmo."));
        });
    }


    [Test]
    public async Task AddAnnotationAsync_ShouldFail_WhenConsultationIsNotInProgress()
    {
        using (var context = new EiEngsofContext(_contextOptions))
        {
            context.UtilizadorConsulta.Add(new UtilizadorConsulta { IdUtilizador = 1, IdConsulta = 5, IsCriador = true });
            context.Estados.Add(new Estado { IdConsulta = 5, EstadoTo = "Agendada", DhRegisto = DateTime.Now });
            await context.SaveChangesAsync();
        }

        var (ok, message) = await _service.AddAnnotationAsync(1, 5, "Nota de teste");

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.False);
            Assert.That(message, Is.EqualTo("Só é possível adicionar anotações quando a consulta está em andamento."));
        });
    }


    [Test]
    public async Task EndConsultationAsync_ShouldSetEndDateAndAddEncerradaState()
    {
        using (var context = new EiEngsofContext(_contextOptions))
        {
            context.Consulta.Add(new Consulta { IdConsulta = 20, DhInicio = DateTime.Now });
            context.UtilizadorConsulta.Add(new UtilizadorConsulta { IdUtilizador = 1, IdConsulta = 20, IsCriador = true });
            await context.SaveChangesAsync();
        }

        var success = await _service.EndConsultationAsync(1, 20);
        Assert.That(success, Is.True);

        using (var context = new EiEngsofContext(_contextOptions))
        {
            var consulta = await context.Consulta.Include(c => c.Estados).FirstAsync(c => c.IdConsulta == 20);
            
            Assert.Multiple(() =>
            {
                Assert.That(consulta.DhFim, Is.Not.Null);
                Assert.That(consulta.Estados, Has.Some.Matches<Estado>(e => e.EstadoTo == "Encerrada"));
            });
        }
    }
}