using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class CodigoPostal
{
    public string CodPostal { get; set; } = null!;

    public string Localidade { get; set; } = null!;

    public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();

    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
