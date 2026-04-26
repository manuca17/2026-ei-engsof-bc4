using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Services;

public class ConsultasService
{
    private readonly IDbContextFactory<EiEngsofContext> _factory;

    public ConsultasService(IDbContextFactory<EiEngsofContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<PatientOptionItem>> GetPatientsForDoctorAsync(int idUtilizador)
    {
        if (idUtilizador <= 0)
        {
            return [];
        }

        await using var context = await _factory.CreateDbContextAsync();

        var patientIds = context.UtilizadorConsulta
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation.IdPaciente)
            .Where(idPaciente => idPaciente.HasValue)
            .Select(idPaciente => idPaciente!.Value)
            .Distinct();

        return await context.Pacientes
            .AsNoTracking()
            // Include both patients created by this doctor and linked-by-consultation patients.
            .Where(p => p.IdUtilizador == idUtilizador || patientIds.Contains(p.IdPaciente))
            .OrderBy(p => p.Nome)
            .Select(p => new PatientOptionItem
            {
                Id = p.IdPaciente,
                Name = p.Nome
            })
            .ToListAsync();
    }

    public async Task<List<ConsultationListItem>> GetForDoctorAsync(int idUtilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation)
            .OrderByDescending(c => c.DhInicio)
            .Select(c => new ConsultationListItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                Status = MapStatus(c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault()),
                ChargingType = c.ValorHora.HasValue && c.ValorHora.Value > 0
                    ? ChargingType.PorHora
                    : ChargingType.Fixo,
                FixedPrice = c.ValorTotal,
                HourlyPrice = c.ValorHora,
                StartAt = c.DhInicio,
                ExamsCount = c.ExamesDaConsulta.Count(),
                NotesCount = c.Anotacaos.Count(),
                InvitesCount = c.UtilizadorConsulta.Count(uc => !uc.IsCriador)
            })
            .ToListAsync();
    }

    public async Task<ConsultationListItem?> GetByIdForDoctorAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite) && link.IdConsulta == idConsulta)
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new ConsultationListItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                Status = MapStatus(c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault()),
                ChargingType = c.ValorHora.HasValue && c.ValorHora.Value > 0
                    ? ChargingType.PorHora
                    : ChargingType.Fixo,
                FixedPrice = c.ValorTotal,
                HourlyPrice = c.ValorHora,
                StartAt = c.DhInicio,
                ExamsCount = c.ExamesDaConsulta.Count(),
                NotesCount = c.Anotacaos.Count(),
                InvitesCount = c.UtilizadorConsulta.Count(uc => !uc.IsCriador)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ConsultationDetailItem?> GetDetailForDoctorAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var consulta = await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite) && link.IdConsulta == idConsulta)
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new
            {
                Consulta = c,
                NomePaciente = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                LatestEstado = c.Estados.OrderByDescending(e => e.DhRegisto).FirstOrDefault(),
                Owner = c.UtilizadorConsulta
                    .Where(uc => uc.IsCriador)
                    .Select(uc => new
                    {
                        uc.IdUtilizador,
                        Nome = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Nome : "Médico"
                    })
                    .FirstOrDefault(),
                Notes = c.Anotacaos
                    .OrderByDescending(n => n.DhCriacao)
                    .Select(n => new DetailAnnotationItem
                    {
                        Id = n.IdAnotacao,
                        Text = n.Descricao ?? string.Empty,
                        CreatedAt = n.DhCriacao,
                        UserId = n.IdUtilizador,
                        UserName = n.IdUtilizadorNavigation != null ? n.IdUtilizadorNavigation.Nome : "Utilizador"
                    })
                    .ToList(),
                Invites = c.UtilizadorConsulta
                    .Where(uc => !uc.IsCriador)
                    .Select(uc => new DetailInviteItem
                    {
                        UserId = uc.IdUtilizador,
                        UserName = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Nome : "Utilizador",
                        UserEmail = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Email : null,
                        IsAccepted = uc.ConviteAceite
                    })
                    .ToList(),
                    CurrentExams = c.ExamesDaConsulta.Count == 0
                        ? new List<DetailExamItem>()
                        : c.ExamesDaConsulta.Select(e => new DetailExamItem
                        {
                            Id = e.IdExameMedicoNavigation.IdExameMedico,
                            Name = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                            Type = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                            Date = e.IdExameMedicoNavigation.DhExame,
                            Description = null,
                            Results = e.IdExameMedicoNavigation.Resultado,
                            FileName = e.IdExameMedicoNavigation.FicheiroNome,
                            FilePath = e.IdExameMedicoNavigation.FicheiroCaminho
                        }).ToList()
            })
            .FirstOrDefaultAsync();

        if (consulta is null)
        {
            return null;
        }

        List<DetailExamItem> availableExams = [];
        if (consulta.Consulta.IdPaciente.HasValue)
        {
            var currentExamIds = consulta.CurrentExams.Select(e => e.Id).ToHashSet();

            availableExams = await context.ExameMedicoConsulta
                .AsNoTracking()
                .Where(emc => emc.IdConsultaNavigation!.IdPaciente == consulta.Consulta.IdPaciente
                              && !currentExamIds.Contains(emc.IdExameMedico))
                .Select(emc => new DetailExamItem
                {
                    Id = emc.IdExameMedicoNavigation!.IdExameMedico,
                    Name = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Type = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Date = emc.IdExameMedicoNavigation.DhExame,
                    Description = null,
                    Results = emc.IdExameMedicoNavigation.Resultado,
                    FileName = emc.IdExameMedicoNavigation.FicheiroNome,
                    FilePath = emc.IdExameMedicoNavigation.FicheiroCaminho
                })
                .Distinct()
                .ToListAsync();
        }

        return new ConsultationDetailItem
        {
            Id = consulta.Consulta.IdConsulta,
            PatientId = consulta.Consulta.IdPaciente,
            PatientName = consulta.NomePaciente,
            Description = consulta.LatestEstado?.Comentario ?? "Consulta médica",
            Status = MapStatus(consulta.LatestEstado?.EstadoTo),
            ChargingType = consulta.Consulta.ValorHora.HasValue && consulta.Consulta.ValorHora.Value > 0 ? ChargingType.PorHora : ChargingType.Fixo,
            FixedPrice = consulta.Consulta.ValorTotal,
            HourlyPrice = consulta.Consulta.ValorHora,
            StartAt = consulta.Consulta.DhInicio,
            EndAt = consulta.Consulta.DhFim,
            OwnerUserId = consulta.Owner != null ? consulta.Owner.IdUtilizador : null,
            OwnerUserName = consulta.Owner != null ? consulta.Owner.Nome : "Médico",
            Exams = consulta.CurrentExams,
            AvailableExams = availableExams,
            Annotations = consulta.Notes,
            Invites = consulta.Invites
        };
    }

    public async Task<ConsultationDetailItem?> GetInviteDetailAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var consulta = await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && !link.IsCriador && link.IdConsulta == idConsulta)
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new
            {
                Consulta = c,
                NomePaciente = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Paciente sem nome",
                LatestEstado = c.Estados.OrderByDescending(e => e.DhRegisto).FirstOrDefault(),
                Owner = c.UtilizadorConsulta
                    .Where(uc => uc.IsCriador)
                    .Select(uc => new
                    {
                        uc.IdUtilizador,
                        Nome = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Nome : "Médico"
                    })
                    .FirstOrDefault(),
                Notes = c.Anotacaos
                    .OrderByDescending(n => n.DhCriacao)
                    .Select(n => new DetailAnnotationItem
                    {
                        Id = n.IdAnotacao,
                        Text = n.Descricao ?? string.Empty,
                        CreatedAt = n.DhCriacao,
                        UserId = n.IdUtilizador,
                        UserName = n.IdUtilizadorNavigation != null ? n.IdUtilizadorNavigation.Nome : "Utilizador"
                    })
                    .ToList(),
                Invites = c.UtilizadorConsulta
                    .Where(uc => !uc.IsCriador)
                    .Select(uc => new DetailInviteItem
                    {
                        UserId = uc.IdUtilizador,
                        UserName = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Nome : "Utilizador",
                        UserEmail = uc.IdUtilizadorNavigation != null ? uc.IdUtilizadorNavigation.Email : null,
                        IsAccepted = uc.ConviteAceite
                    })
                    .ToList(),
                CurrentExams = c.ExamesDaConsulta.Count == 0
                    ? new List<DetailExamItem>()
                    : c.ExamesDaConsulta.Select(e => new DetailExamItem
                    {
                        Id = e.IdExameMedicoNavigation.IdExameMedico,
                        Name = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                        Type = e.IdExameMedicoNavigation.Tipo ?? "Exame",
                        Date = e.IdExameMedicoNavigation.DhExame,
                        Description = null,
                        Results = e.IdExameMedicoNavigation.Resultado,
                        FileName = e.IdExameMedicoNavigation.FicheiroNome,
                        FilePath = e.IdExameMedicoNavigation.FicheiroCaminho
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (consulta is null)
        {
            return null;
        }

        List<DetailExamItem> availableExams = [];
        if (consulta.Consulta.IdPaciente.HasValue)
        {
            var currentExamIds = consulta.CurrentExams.Select(e => e.Id).ToHashSet();

            availableExams = await context.ExameMedicoConsulta
                .AsNoTracking()
                .Where(emc => emc.IdConsultaNavigation!.IdPaciente == consulta.Consulta.IdPaciente
                              && !currentExamIds.Contains(emc.IdExameMedico))
                .Select(emc => new DetailExamItem
                {
                    Id = emc.IdExameMedicoNavigation!.IdExameMedico,
                    Name = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Type = emc.IdExameMedicoNavigation.Tipo ?? "Exame",
                    Date = emc.IdExameMedicoNavigation.DhExame,
                    Description = null,
                    Results = emc.IdExameMedicoNavigation.Resultado
                })
                .Distinct()
                .ToListAsync();
        }

        return new ConsultationDetailItem
        {
            Id = consulta.Consulta.IdConsulta,
            PatientId = consulta.Consulta.IdPaciente,
            PatientName = consulta.NomePaciente,
            Description = consulta.LatestEstado?.Comentario ?? "Consulta médica",
            Status = MapStatus(consulta.LatestEstado?.EstadoTo),
            ChargingType = consulta.Consulta.ValorHora.HasValue && consulta.Consulta.ValorHora.Value > 0 ? ChargingType.PorHora : ChargingType.Fixo,
            FixedPrice = consulta.Consulta.ValorTotal,
            HourlyPrice = consulta.Consulta.ValorHora,
            StartAt = consulta.Consulta.DhInicio,
            EndAt = consulta.Consulta.DhFim,
            OwnerUserId = consulta.Owner != null ? consulta.Owner.IdUtilizador : null,
            OwnerUserName = consulta.Owner != null ? consulta.Owner.Nome : "Médico",
            Exams = consulta.CurrentExams,
            AvailableExams = availableExams,
            Annotations = consulta.Notes,
            Invites = consulta.Invites
        };
    }

    public async Task<(bool Ok, string Message)> SendInviteAsync(int idUtilizador, int idConsulta, string inviteEmail)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var isCreator = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && link.IsCriador);
        
        if (!isCreator)
        {
            return (false, "Só o criador da consulta pode enviar convites.");
        }

        if (string.IsNullOrWhiteSpace(inviteEmail))
        {
            return (false, "Introduza um email válido.");
        }

        var invitedUser = await context.Utilizadores.FirstOrDefaultAsync(u => u.Email == inviteEmail.Trim());
        if (invitedUser is null)
        {
            return (false, "Utilizador não encontrado.");
        }

        if (invitedUser.IdUtilizador == idUtilizador)
        {
            return (false, "Não pode convidar-se a si mesmo.");
        }

        var existingInvite = await context.UtilizadorConsulta
            .FirstOrDefaultAsync(link =>
                link.IdConsulta == idConsulta &&
                link.IdUtilizador == invitedUser.IdUtilizador &&
                !link.IsCriador);

        if (existingInvite is not null)
        {
            return existingInvite.ConviteAceite
                ? (false, "O utilizador já participa nesta consulta.")
                : (false, "Já existe um convite pendente para este utilizador.");
        }

        var latestStatus = await context.Estados
            .Where(e => e.IdConsulta == idConsulta)
            .OrderByDescending(e => e.DhRegisto)
            .Select(e => e.EstadoTo)
            .FirstOrDefaultAsync();

        if (MapStatus(latestStatus).Equals(ConsultationStatus.Encerrada))
        {
            return (false, "Não é possível convidar utilizadores para uma consulta encerrada.");
        }

        context.UtilizadorConsulta.Add(new UtilizadorConsulta
        {
            IdConsulta = idConsulta,
            IdUtilizador = invitedUser.IdUtilizador,
            IsCriador = false,
            ConviteAceite = false
        });

        await context.SaveChangesAsync();
        return (true, $"Convite enviado para {invitedUser.Nome}.");
    }

    public async Task<(bool Ok, string Message)> AcceptInviteAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var invite = await context.UtilizadorConsulta
            .FirstOrDefaultAsync(link =>
                link.IdConsulta == idConsulta &&
                link.IdUtilizador == idUtilizador &&
                !link.IsCriador);

        if (invite is null)
        {
            return (false, "Convite não encontrado.");
        }

        if (invite.ConviteAceite)
        {
            return (false, "Convite já aceite.");
        }

        invite.ConviteAceite = true;
        await context.SaveChangesAsync();
        return (true, "Convite aceite com sucesso.");
    }

    public async Task<(bool Ok, string Message)> RemoveInviteAsync(int idUtilizador, int idConsulta, int invitedUserId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var isCreator = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && link.IsCriador);
        
        if (!isCreator)
        {
            return (false, "Só o criador da consulta pode remover convites.");
        }

        var invite = await context.UtilizadorConsulta
            .FirstOrDefaultAsync(link =>
                link.IdConsulta == idConsulta &&
                link.IdUtilizador == invitedUserId &&
                !link.IsCriador);

        if (invite is null)
        {
            return (false, "Convite não encontrado.");
        }

        context.UtilizadorConsulta.Remove(invite);
        await context.SaveChangesAsync();
        return (true, "Convite removido com sucesso.");
    }

    public async Task<(bool Ok, string Message)> DeclineInviteAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var invite = await context.UtilizadorConsulta
            .FirstOrDefaultAsync(link =>
                link.IdConsulta == idConsulta &&
                link.IdUtilizador == idUtilizador &&
                !link.IsCriador &&
                !link.ConviteAceite);

        if (invite is null)
        {
            return (false, "Convite pendente não encontrado.");
        }

        context.UtilizadorConsulta.Remove(invite);
        await context.SaveChangesAsync();
        return (true, "Convite recusado.");
    }

    public async Task<List<PendingInviteItem>> GetPendingInvitesForDoctorAsync(int idUtilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && !link.IsCriador && !link.ConviteAceite)
            .Select(link => new PendingInviteItem
            {
                IdConsulta = link.IdConsulta,
                PatientName = link.IdConsultaNavigation != null && link.IdConsultaNavigation.IdPacienteNavigation != null
                    ? link.IdConsultaNavigation.IdPacienteNavigation.Nome
                    : "Paciente sem nome",
                OwnerName = link.IdConsultaNavigation != null
                    ? link.IdConsultaNavigation.UtilizadorConsulta
                        .Where(ownerLink => ownerLink.IsCriador)
                        .Select(ownerLink => ownerLink.IdUtilizadorNavigation != null ? ownerLink.IdUtilizadorNavigation.Nome : "Médico")
                        .FirstOrDefault() ?? "Médico"
                    : "Médico",
                StartAt = link.IdConsultaNavigation != null ? link.IdConsultaNavigation.DhInicio : null
            })
            .OrderByDescending(item => item.StartAt ?? DateTime.MinValue)
            .ToListAsync();
    }

    public async Task<bool> StartConsultationAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return false;
        }

        context.Estados.Add(new Estado
        {
            IdConsulta = idConsulta,
            EstadoTo = "Em andamento",
            Comentario = "Consulta iniciada",
            DhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EndConsultationAsync(int idUtilizador, int idConsulta)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return false;
        }

        var consulta = await context.Consulta.FirstOrDefaultAsync(c => c.IdConsulta == idConsulta);
        if (consulta is null)
        {
            return false;
        }

        consulta.DhFim = DateTime.Now;
        context.Estados.Add(new Estado
        {
            IdConsulta = idConsulta,
            EstadoTo = "Encerrada",
            Comentario = "Consulta encerrada",
            DhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Ok, string Message)> AddNewExamAsync(int idUtilizador, int idConsulta, DateTime date, string type, string? results, string? fileName, string? filePath)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var exame = new ExameMedico
        {
            DhExame = date,
            Tipo = type,
            Resultado = results,
            FicheiroNome = fileName,
            FicheiroCaminho = filePath,
            IdUtilizador = idUtilizador
        };
        
        Console.WriteLine($"[ADD EXAM] Adding new exam for consulta {idConsulta} by user {idUtilizador} with type {type} and date {date} and results {(string.IsNullOrWhiteSpace(results) ? "null/empty" : "provided")} and fileName {(string.IsNullOrWhiteSpace(fileName) ? "null/empty" : "provided")} and filePath {(string.IsNullOrWhiteSpace(filePath) ? "null/empty" : "provided")}");
        context.ExameMedicos.Add(exame);
        await context.SaveChangesAsync();
        Console.WriteLine($"[ADD EXAM] New exam added with id {exame.IdExameMedico} for consulta {idConsulta} by user {idUtilizador} with type {type} and date {date} and results {(string.IsNullOrWhiteSpace(results) ? "null/empty" : "provided")} and fileName {(string.IsNullOrWhiteSpace(fileName) ? "null/empty" : "provided")} and filePath {(string.IsNullOrWhiteSpace(filePath) ? "null/empty" : "provided")}");
        context.ExameMedicoConsulta.Add(new ExameMedicoConsulta
        {
            IdExameMedico = exame.IdExameMedico,
            IdConsulta = idConsulta,
            IdUtilzador = idUtilizador,
            dhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Exame adicionado com sucesso");
    }

    public async Task<(bool Ok, string Message)> AssociateExamAsync(int idUtilizador, int idConsulta, int idExame)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var alreadyLinked = await context.ExameMedicoConsulta.AnyAsync(e =>
            e.IdConsulta == idConsulta && e.IdExameMedico == idExame);
        if (alreadyLinked)
        {
            return (false, "Este exame já está associado à consulta.");
        }

        var examExists = await context.ExameMedicos.AnyAsync(e => e.IdExameMedico == idExame);
        if (!examExists)
        {
            return (false, "Exame não encontrado.");
        }

        context.ExameMedicoConsulta.Add(new ExameMedicoConsulta
        {
            IdExameMedico = idExame,
            IdConsulta = idConsulta,
            IdUtilzador = idUtilizador,
            dhRegisto = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Exame associado com sucesso");
    }

    public async Task<(bool Ok, string Message)> RemoveExamAsync(int idUtilizador, int idConsulta, int idExameMedico)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var examConsultaLink = await context.ExameMedicoConsulta
            .FirstOrDefaultAsync(e => e.IdConsulta == idConsulta && e.IdExameMedico == idExameMedico);

        if (examConsultaLink is null)
        {
            return (false, "Associação de exame não encontrada.");
        }

        context.ExameMedicoConsulta.Remove(examConsultaLink);
        await context.SaveChangesAsync();
        return (true, "Exame removido com sucesso.");
    }

    public async Task<(bool Ok, string Message)> AddAnnotationAsync(int idUtilizador, int idConsulta, string text)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador && link.IdConsulta == idConsulta && (link.IsCriador || link.ConviteAceite));
        if (!hasAccess)
        {
            return (false, "Sem permissões para alterar esta consulta.");
        }

        var latestStatus = await context.Estados
            .Where(e => e.IdConsulta == idConsulta)
            .OrderByDescending(e => e.DhRegisto)
            .Select(e => e.EstadoTo)
            .FirstOrDefaultAsync();

        if (!MapStatus(latestStatus).Equals(ConsultationStatus.EmAndamento))
        {
            return (false, "Só é possível adicionar anotações quando a consulta está em andamento.");
        }

        context.Anotacaos.Add(new Anotacao
        {
            IdConsulta = idConsulta,
            IdUtilizador = idUtilizador,
            Descricao = text,
            DhCriacao = DateTime.Now
        });

        await context.SaveChangesAsync();
        return (true, "Anotação adicionada com sucesso.");
    }

    public async Task<(bool Ok, string? ErrorMessage, int? IdConsulta)> CreateForDoctorAsync(CreateConsultaRequest request)
    {
        await using var context = await _factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var consulta = new Consulta
            {
                DhInicio = DateTime.Now,
                IdPaciente = request.IdPaciente,
                ValorTotal = request.ChargingType == ChargingType.Fixo ? request.FixedPrice : null,
                ValorHora = request.ChargingType == ChargingType.PorHora ? request.HourlyPrice : null
            };

            context.Consulta.Add(consulta);
            await context.SaveChangesAsync();

            context.UtilizadorConsulta.Add(new UtilizadorConsulta
            {
                IdUtilizador = request.IdUtilizador,
                IdConsulta = consulta.IdConsulta,
                IsCriador = true,
                ConviteAceite = true
            });

            context.Estados.Add(new Estado
            {
                IdConsulta = consulta.IdConsulta,
                EstadoTo = "Agendada",
                Comentario = request.Description,
                DhRegisto = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null, consulta.IdConsulta);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Ok, string? ErrorMessage)> UpdateForDoctorAsync(int idUtilizador, int idConsulta, UpdateConsultaRequest request)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var hasAccess = await context.UtilizadorConsulta.AnyAsync(link =>
            link.IdUtilizador == idUtilizador &&
            link.IdConsulta == idConsulta &&
            (link.IsCriador || link.ConviteAceite));

        if (!hasAccess)
        {
            return (false, "Sem permissões para editar esta consulta.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return (false, "A descrição é obrigatória.");
        }

        if (request.ChargingType == ChargingType.Fixo && (!request.FixedPrice.HasValue || request.FixedPrice <= 0))
        {
            return (false, "Introduza um preço fixo válido.");
        }

        if (request.ChargingType == ChargingType.PorHora && (!request.HourlyPrice.HasValue || request.HourlyPrice <= 0))
        {
            return (false, "Introduza um preço por hora válido.");
        }

        var consulta = await context.Consulta
            .Include(c => c.Estados)
            .FirstOrDefaultAsync(c => c.IdConsulta == idConsulta);

        if (consulta is null)
        {
            return (false, "Consulta não encontrada.");
        }

        var latestEstado = consulta.Estados
            .OrderByDescending(e => e.DhRegisto)
            .FirstOrDefault();

        var normalizedStatus = (latestEstado?.EstadoTo ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedStatus.Contains("encerr"))
        {
            return (false, "Não é possível editar uma consulta encerrada.");
        }

        consulta.ValorTotal = request.ChargingType == ChargingType.Fixo ? request.FixedPrice : null;
        consulta.ValorHora = request.ChargingType == ChargingType.PorHora ? request.HourlyPrice : null;

        if (latestEstado is null)
        {
            context.Estados.Add(new Estado
            {
                IdConsulta = idConsulta,
                EstadoTo = "Agendada",
                Comentario = request.Description.Trim(),
                DhRegisto = DateTime.Now
            });
        }
        else
        {
            latestEstado.Comentario = request.Description.Trim();
            latestEstado.DhRegisto = DateTime.Now;
        }

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<ReportConsultationItem>> GetMonthlyReportAsync(int idUtilizador, int year, int month)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var startOfMonth = new DateTime(year, month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && link.IsCriador)
            .Select(link => link.IdConsultaNavigation)
            .Where(c => c.DhFim.HasValue && c.DhFim >= startOfMonth && c.DhFim < startOfNextMonth)
            .Select(c => new ReportConsultationItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente ?? 0,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Sem paciente",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                DhInicio = c.DhInicio,
                DhFim = c.DhFim,
                ValorTotal = c.ValorTotal,
                ValorHora = c.ValorHora,
                IsHourly = c.ValorHora.HasValue && c.ValorHora.Value > 0
            })
            .OrderBy(c => c.DhFim)
            .ToListAsync();
    }

    public async Task<List<ReportConsultationItem>> GetPatientMonthlyReportAsync(int idUtilizador, int patientId, int year, int month)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var startOfMonth = new DateTime(year, month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        return await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation)
            .Where(c => c.IdPaciente == patientId && c.DhFim.HasValue && c.DhFim >= startOfMonth && c.DhFim < startOfNextMonth)
            .Select(c => new ReportConsultationItem
            {
                Id = c.IdConsulta,
                PatientId = c.IdPaciente ?? 0,
                PatientName = c.IdPacienteNavigation != null ? c.IdPacienteNavigation.Nome : "Sem paciente",
                Description = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.Comentario)
                    .FirstOrDefault() ?? "Consulta médica",
                DhInicio = c.DhInicio,
                DhFim = c.DhFim,
                ValorTotal = c.ValorTotal,
                ValorHora = c.ValorHora,
                IsHourly = c.ValorHora.HasValue && c.ValorHora.Value > 0
            })
            .OrderBy(c => c.DhFim)
            .ToListAsync();
    }

    public sealed class ReportConsultationItem
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime DhInicio { get; init; }
        public DateTime? DhFim { get; init; }
        public decimal? ValorTotal { get; init; }
        public decimal? ValorHora { get; init; }
        public bool IsHourly { get; init; }

        public double Hours => DhFim.HasValue
            ? Math.Max(0d, (DhFim.Value - DhInicio).TotalHours)
            : 0d;

        public decimal Price => IsHourly
            ? Math.Round((decimal)Hours * (ValorHora ?? 0m), 2)
            : (ValorTotal ?? 0m);
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(int idUtilizador)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var consultations = await context.UtilizadorConsulta
            .AsNoTracking()
            .Where(link => link.IdUtilizador == idUtilizador && (link.IsCriador || link.ConviteAceite))
            .Select(link => link.IdConsultaNavigation)
            .Select(c => new
            {
                c.IdConsulta,
                c.DhInicio,
                c.DhFim,
                c.ValorTotal,
                c.ValorHora,
                LatestStatus = c.Estados
                    .OrderByDescending(e => e.DhRegisto)
                    .Select(e => e.EstadoTo)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var total = consultations.Count;

        var pending = consultations.Count(c =>
        {
            var status = MapStatus(c.LatestStatus);
            return status == ConsultationStatus.Agendada || status == ConsultationStatus.EmAndamento;
        });

        decimal monthlyRevenue = 0m;
        foreach (var c in consultations.Where(c => c.DhInicio >= startOfMonth && c.DhInicio < startOfNextMonth))
        {
            if (c.ValorHora.HasValue && c.ValorHora.Value > 0)
            {
                if (c.DhFim.HasValue)
                {
                    var hours = (decimal)(c.DhFim.Value - c.DhInicio).TotalHours;
                    monthlyRevenue += c.ValorHora.Value * Math.Max(0, hours);
                }
            }
            else if (c.ValorTotal.HasValue)
            {
                monthlyRevenue += c.ValorTotal.Value;
            }
        }

        return new DashboardStats
        {
            TotalConsultations = total,
            PendingConsultations = pending,
            MonthlyRevenue = monthlyRevenue
        };
    }

    public sealed class DashboardStats
    {
        public int TotalConsultations { get; init; }
        public int PendingConsultations { get; init; }
        public decimal MonthlyRevenue { get; init; }
    }

    private static ConsultationStatus MapStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Contains("encerr"))
        {
            return ConsultationStatus.Encerrada;
        }

        if (normalized.Contains("andamento") || normalized.Contains("curso"))
        {
            return ConsultationStatus.EmAndamento;
        }

        return ConsultationStatus.Agendada;
    }

    public enum ConsultationStatus
    {
        Agendada,
        EmAndamento,
        Encerrada
    }

    public enum ChargingType
    {
        Fixo,
        PorHora
    }

    public sealed class PatientOptionItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public sealed class ConsultationListItem
    {
        public int Id { get; init; }
        public int? PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ConsultationStatus Status { get; init; }
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
        public DateTime? StartAt { get; init; }
        public int ExamsCount { get; init; }
        public int NotesCount { get; init; }
        public int InvitesCount { get; init; }
    }

    public sealed class CreateConsultaRequest
    {
        public int IdUtilizador { get; init; }
        public int IdPaciente { get; init; }
        public string Description { get; init; } = string.Empty;
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
    }

    public sealed class UpdateConsultaRequest
    {
        public string Description { get; init; } = string.Empty;
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
    }

    public sealed class ConsultationDetailItem
    {
        public int Id { get; init; }
        public int? PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ConsultationStatus Status { get; init; }
        public ChargingType ChargingType { get; init; }
        public decimal? FixedPrice { get; init; }
        public decimal? HourlyPrice { get; init; }
        public DateTime? StartAt { get; init; }
        public DateTime? EndAt { get; init; }
        public int? OwnerUserId { get; init; }
        public string OwnerUserName { get; init; } = string.Empty;
        public List<DetailExamItem> Exams { get; init; } = [];
        public List<DetailExamItem> AvailableExams { get; init; } = [];
        public List<DetailAnnotationItem> Annotations { get; init; } = [];
        public List<DetailInviteItem> Invites { get; init; } = [];
    }

    public sealed class DetailInviteItem
    {
        public int UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string? UserEmail { get; init; }
        public bool IsAccepted { get; init; }
    }

    public sealed class DetailExamItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public string? Description { get; init; }
        public string? Results { get; init; }
        public string? FileName { get; init; }
        public string? FilePath { get; init; }
    }

    public sealed class DetailAnnotationItem
    {
        public int Id { get; init; }
        public string Text { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public int? UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
    }

    public sealed class PendingInviteItem
    {
        public int IdConsulta { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string OwnerName { get; init; } = string.Empty;
        public DateTime? StartAt { get; init; }
    }
}