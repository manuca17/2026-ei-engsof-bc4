using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class UtilizadorConsulta
{
    public int IdUtilizador { get; set; }

    public int IdConsulta { get; set; }

    public bool IsCriador { get; set; }

    public bool ConviteAceite { get; set; }

    public virtual Consulta IdConsultaNavigation { get; set; } = null!;

    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
}
