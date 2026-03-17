using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Estado
{
    public int IdEstado { get; set; }

    public string EstadoTo { get; set; } = null!;

    public DateTime DhRegisto { get; set; }

    public string? Comentario { get; set; }

    public int IdConsulta { get; set; }

    public virtual Consulta IdConsultaNavigation { get; set; } = null!;
}
