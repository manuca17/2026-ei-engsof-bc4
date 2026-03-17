using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Utilizador
{
    public int IdUtilizador { get; set; }

    public bool IsManager { get; set; }

    public bool IsAdmin { get; set; }

    public string Nome { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? NumPorta { get; set; }

    public string? Rua { get; set; }

    public string? CodPostal { get; set; }

    public string? NumCarteira { get; set; }

    public string? Especialidade { get; set; }

    public virtual ICollection<Anotacao> Anotacaos { get; set; } = new List<Anotacao>();

    public virtual CodigoPostal? CodPostalNavigation { get; set; }

    public virtual ICollection<ExameMedico> ExameMedicos { get; set; } = new List<ExameMedico>();

    public virtual ICollection<TipoConsulta> TipoConsulta { get; set; } = new List<TipoConsulta>();

    public virtual ICollection<UtilizadorConsulta> UtilizadorConsulta { get; set; } = new List<UtilizadorConsulta>();
}
