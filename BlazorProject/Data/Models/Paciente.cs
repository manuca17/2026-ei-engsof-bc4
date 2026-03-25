using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Paciente
{
    public int IdPaciente { get; set; }

    public int? IdUtilizador { get; set; }

    public string Nome { get; set; } = null!;

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? NumPorta { get; set; }

    public string? Rua { get; set; }

    public string? CodPostal { get; set; }

    
    public string? Nif { get; set; }

    public DateOnly? DtNasc { get; set; }

    public virtual CodigoPostal? CodPostalNavigation { get; set; }

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
    
    public virtual ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();
}
