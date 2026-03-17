using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Consulta
{
    public int IdConsulta { get; set; }

    public DateTime DhInicio { get; set; }

    public DateTime? DhFim { get; set; }

    public decimal? ValorTotal { get; set; }

    public decimal? ValorHora { get; set; }

    public int? IdPaciente { get; set; }

    public int? IdExameMedico { get; set; }

    public int? IdFatura { get; set; }

    public int? IdTipoConsulta { get; set; }

    public virtual ICollection<Anotacao> Anotacaos { get; set; } = new List<Anotacao>();

    public virtual ICollection<Estado> Estados { get; set; } = new List<Estado>();

    public virtual ExameMedico? IdExameMedicoNavigation { get; set; }

    public virtual Fatura? IdFaturaNavigation { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }

    public virtual TipoConsulta? IdTipoConsultaNavigation { get; set; }

    public virtual ICollection<UtilizadorConsulta> UtilizadorConsulta { get; set; } = new List<UtilizadorConsulta>();
}
