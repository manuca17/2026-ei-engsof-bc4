using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class ExameMedico
{
    public int IdExameMedico { get; set; }

    public DateTime DhExame { get; set; }

    public string? Tipo { get; set; }

    public string? Resultado { get; set; }

    public int? IdUtilizador { get; set; }

    public virtual ICollection<Consultum> Consulta { get; set; } = new List<Consultum>();

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
}
