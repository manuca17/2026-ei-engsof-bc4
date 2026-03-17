using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorProject.Services;

public sealed class ConsultationStore
{
    public IReadOnlyList<UserItem> Users => _users;
    public IReadOnlyList<PatientItem> Patients => _patients;
    public IReadOnlyList<MedicalExamItem> MedicalExams => _medicalExams;
    public IReadOnlyList<ConsultationItem> Consultations => _consultations;

    private readonly List<UserItem> _users =
    [
        new() { Id = "u1", Name = "Dr. João Silva", Email = "joao@example.com", Role = "medico", Specialty = "Cardiologia", Phone = "+351 912 345 678" },
        new() { Id = "u2", Name = "Admin", Email = "admin@example.com", Role = "admin", Specialty = null, Phone = null },
        new() { Id = "u3", Name = "Dra. Maria Costa", Email = "maria@example.com", Role = "medico", Specialty = "Pediatria", Phone = "+351 913 111 222" },
    ];

    private readonly List<PatientItem> _patients =
    [
        new() { Id = "p1", Name = "João Mendes" },
        new() { Id = "p2", Name = "Ana Costa" },
        new() { Id = "p3", Name = "Rui Alves" },
    ];

    private readonly List<MedicalExamItem> _medicalExams;
    private readonly List<ConsultationItem> _consultations;

    public ConsultationStore()
    {
        var exam1 = new MedicalExamItem
        {
            Id = "e1",
            PatientId = "p1",
            Name = "Hemograma",
            Type = "Sangue",
            Date = DateTime.Now.AddDays(-10),
            Description = "Rotina",
            Results = "Valores dentro do normal",
            CreatedAt = DateTime.Now.AddDays(-10)
        };

        var exam2 = new MedicalExamItem
        {
            Id = "e2",
            PatientId = "p1",
            Name = "Ecocardiograma",
            Type = "Imagem",
            Date = DateTime.Now.AddDays(-30),
            Description = "Avaliação cardíaca complementar",
            Results = "Sem alterações relevantes",
            CreatedAt = DateTime.Now.AddDays(-30)
        };

        var exam3 = new MedicalExamItem
        {
            Id = "e3",
            PatientId = "p2",
            Name = "Eletrocardiograma",
            Type = "Cardiologia",
            Date = DateTime.Now.AddDays(-5),
            Description = "Exame de rotina",
            Results = "Ritmo sinusal regular",
            CreatedAt = DateTime.Now.AddDays(-5)
        };

        _medicalExams = [exam1, exam2, exam3];

        _consultations =
        [
            new ConsultationItem
            {
                Id = "c1",
                PatientId = "p1",
                PatientName = "João Mendes",
                Description = "Avaliação de rotina",
                Status = ConsultationStatus.Agendada,
                ChargingType = ChargingType.Fixo,
                FixedPrice = 80m,
                StartAt = DateTime.Now.AddDays(1),
                OwnerUserId = "u1",
                OwnerUserName = "Dr. João Silva",
                Exams = [exam1],
                Annotations =
                [
                    new AnnotationItem
                    {
                        Id = "n1",
                        Text = "Paciente refere melhora dos sintomas.",
                        CreatedAt = DateTime.Now.AddDays(-2),
                        UserId = "u1",
                        UserName = "Dr. João Silva"
                    }
                ]
            },
            new ConsultationItem
            {
                Id = "c2",
                PatientId = "p2",
                PatientName = "Ana Costa",
                Description = "Revisão cardiologia",
                Status = ConsultationStatus.Encerrada,
                ChargingType = ChargingType.PorHora,
                HourlyPrice = 60m,
                StartAt = DateTime.Now.AddDays(-3),
                EndAt = DateTime.Now.AddDays(-3).AddHours(1),
                OwnerUserId = "u1",
                OwnerUserName = "Dr. João Silva",
                Exams = [exam3],
                Invites =
                [
                    new ConsultationInviteItem
                    {
                        Id = "i1",
                        UserId = "u3",
                        UserName = "Dra. Maria Costa",
                        UserEmail = "maria@example.com",
                        Status = InviteStatus.Accepted,
                        Annotations =
                        [
                            new AnnotationItem
                            {
                                Id = "n2",
                                Text = "Ok, posso rever os exames anteriores.",
                                CreatedAt = DateTime.Now.AddDays(-3).AddMinutes(20),
                                UserId = "u3",
                                UserName = "Dra. Maria Costa"
                            }
                        ]
                    }
                ]
            },
            new ConsultationItem
            {
                Id = "c3",
                PatientId = "p3",
                PatientName = "Rui Alves",
                Description = "Consulta de seguimento",
                Status = ConsultationStatus.EmAndamento,
                ChargingType = ChargingType.Fixo,
                FixedPrice = 95m,
                StartAt = DateTime.Now,
                OwnerUserId = "u1",
                OwnerUserName = "Dr. João Silva"
            }
        ];
    }

    public ConsultationItem? GetById(string id) =>
        _consultations.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<MedicalExamItem> GetExamsByPatient(string patientId) =>
        _medicalExams
            .Where(exam => string.Equals(exam.PatientId, patientId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(exam => exam.Date)
            .ToList();

    public UserItem? FindUser(string? email, string? name)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var matchByEmail = _users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matchByEmail is not null)
            {
                return matchByEmail;
            }
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return _users.FirstOrDefault(u => string.Equals(u.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    public void CreateConsultation(string patientId, string description, ChargingType chargingType, decimal? fixedPrice, decimal? hourlyPrice)
    {
        var patient = _patients.FirstOrDefault(p => p.Id == patientId);
        var patientName = patient?.Name ?? "Paciente";

        _consultations.Insert(0, new ConsultationItem
        {
            Id = Guid.NewGuid().ToString("N"),
            PatientId = patientId,
            PatientName = patientName,
            Description = string.IsNullOrWhiteSpace(description) ? "Sem descrição" : description.Trim(),
            Status = ConsultationStatus.Agendada,
            ChargingType = chargingType,
            FixedPrice = chargingType == ChargingType.Fixo ? fixedPrice : null,
            HourlyPrice = chargingType == ChargingType.PorHora ? hourlyPrice : null,
            StartAt = DateTime.Now,
            OwnerUserId = "u1",
            OwnerUserName = "Dr. João Silva",
        });
    }

    public bool StartConsultation(string id)
    {
        var consultation = GetById(id);
        if (consultation is null || consultation.Status != ConsultationStatus.Agendada)
        {
            return false;
        }

        consultation.Status = ConsultationStatus.EmAndamento;
        consultation.StartAt ??= DateTime.Now;
        return true;
    }

    public bool EndConsultation(string id)
    {
        var consultation = GetById(id);
        if (consultation is null || consultation.Status != ConsultationStatus.EmAndamento)
        {
            return false;
        }

        consultation.Status = ConsultationStatus.Encerrada;
        consultation.EndAt ??= DateTime.Now;
        return true;
    }

    public bool UpdateConsultation(string id, string description, ChargingType chargingType, decimal? fixedPrice, decimal? hourlyPrice)
    {
        var consultation = GetById(id);
        if (consultation is null)
        {
            return false;
        }

        consultation.Description = string.IsNullOrWhiteSpace(description) ? consultation.Description : description.Trim();
        consultation.ChargingType = chargingType;
        consultation.FixedPrice = chargingType == ChargingType.Fixo ? fixedPrice : null;
        consultation.HourlyPrice = chargingType == ChargingType.PorHora ? hourlyPrice : null;
        return true;
    }

    public (bool ok, string message, MedicalExamItem? exam) CreateExam(string consultationId, string name, string type, DateTime date, string? description, string? results)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return (false, "Consulta não encontrada", null);
        }

        var exam = new MedicalExamItem
        {
            Id = Guid.NewGuid().ToString("N"),
            PatientId = consultation.PatientId,
            Name = name.Trim(),
            Type = type.Trim(),
            Date = date,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Results = string.IsNullOrWhiteSpace(results) ? null : results.Trim(),
            CreatedAt = DateTime.Now
        };

        _medicalExams.Insert(0, exam);
        consultation.Exams.Add(exam);
        return (true, "Exame adicionado com sucesso", exam);
    }

    public (bool ok, string message) AssociateExistingExam(string consultationId, string examId)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return (false, "Consulta não encontrada");
        }

        var exam = _medicalExams.FirstOrDefault(item => item.Id == examId && item.PatientId == consultation.PatientId);
        if (exam is null)
        {
            return (false, "Exame não encontrado");
        }

        if (consultation.Exams.Any(existing => existing.Id == exam.Id))
        {
            return (false, "Este exame já está associado à consulta");
        }

        consultation.Exams.Add(exam);
        return (true, "Exame associado com sucesso");
    }

    public bool RemoveExam(string consultationId, string examId)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return false;
        }

        return consultation.Exams.RemoveAll(exam => exam.Id == examId) > 0;
    }

    public AnnotationItem AddAnnotation(string consultationId, AnnotationItem annotation)
    {
        var consultation = GetById(consultationId) ?? throw new InvalidOperationException("Consulta não encontrada");
        consultation.Annotations.Add(annotation);
        return annotation;
    }

    public (bool ok, string message) SendInvite(string consultationId, string email, string? initialMessage)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return (false, "Consulta não encontrada");
        }

        var user = _users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return (false, "Utilizador não encontrado");
        }

        if (user.Id == consultation.OwnerUserId)
        {
            return (false, "Não pode convidar-se a si mesmo");
        }

        if (consultation.Invites.Any(invite => invite.UserId == user.Id))
        {
            return (false, "Utilizador já foi convidado");
        }

        var invite = new ConsultationInviteItem
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            UserName = user.Name,
            UserEmail = user.Email,
            Status = InviteStatus.Pending,
        };

        if (!string.IsNullOrWhiteSpace(initialMessage))
        {
            invite.Annotations.Add(new AnnotationItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Text = initialMessage.Trim(),
                CreatedAt = DateTime.Now,
                UserId = consultation.OwnerUserId,
                UserName = consultation.OwnerUserName
            });
        }

        consultation.Invites.Add(invite);
        return (true, $"Convite enviado para {user.Name}");
    }

    public bool AcceptInvite(string consultationId, string inviteId, string userId)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return false;
        }

        var invite = consultation.Invites.FirstOrDefault(item => item.Id == inviteId && item.UserId == userId);
        if (invite is null || invite.Status != InviteStatus.Pending)
        {
            return false;
        }

        invite.Status = InviteStatus.Accepted;
        return true;
    }

    public bool RemoveInvite(string consultationId, string inviteId)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return false;
        }

        return consultation.Invites.RemoveAll(invite => invite.Id == inviteId) > 0;
    }

    public bool AddInviteAnnotation(string consultationId, string inviteId, string userId, string text)
    {
        var consultation = GetById(consultationId);
        if (consultation is null)
        {
            return false;
        }

        var invite = consultation.Invites.FirstOrDefault(item => item.Id == inviteId && item.UserId == userId);
        if (invite is null || invite.Status != InviteStatus.Accepted)
        {
            return false;
        }

        invite.Annotations.Add(new AnnotationItem
        {
            Id = Guid.NewGuid().ToString("N"),
            Text = text.Trim(),
            CreatedAt = DateTime.Now,
            UserId = userId,
            UserName = _users.FirstOrDefault(u => u.Id == userId)?.Name ?? "Utilizador"
        });

        return true;
    }

    public sealed class UserItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = "medico";
        public string? Specialty { get; init; }
        public string? Phone { get; init; }
    }

    public sealed class PatientItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
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

    public enum InviteStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public sealed class MedicalExamItem
    {
        public string Id { get; init; } = string.Empty;
        public string PatientId { get; init; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Results { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class AnnotationItem
    {
        public string Id { get; init; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public sealed class ConsultationInviteItem
    {
        public string Id { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string UserEmail { get; init; } = string.Empty;
        public InviteStatus Status { get; set; } = InviteStatus.Pending;
        public List<AnnotationItem> Annotations { get; init; } = [];
    }

    public sealed class ConsultationItem
    {
        public string Id { get; init; } = string.Empty;
        public string PatientId { get; init; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string OwnerUserId { get; init; } = string.Empty;
        public string OwnerUserName { get; init; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public ConsultationStatus Status { get; set; } = ConsultationStatus.Agendada;

        public ChargingType ChargingType { get; set; } = ChargingType.Fixo;
        public decimal? FixedPrice { get; set; }
        public decimal? HourlyPrice { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public List<MedicalExamItem> Exams { get; init; } = [];
        public List<AnnotationItem> Annotations { get; init; } = [];
        public List<ConsultationInviteItem> Invites { get; init; } = [];

        public int ExamsCount => Exams.Count;
        public int NotesCount => Annotations.Count;
        public int InvitesCount => Invites.Count;
    }
}
