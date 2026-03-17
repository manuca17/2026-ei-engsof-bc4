using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Anotacao
{
    public int IdAnotacao { get; set; }

    public string? Descricao { get; set; }

    public DateTime DhCriacao { get; set; }

    public DateTime? DhUltimaEdicao { get; set; }

    public int? IdUtilizador { get; set; }

    public int? IdConsulta { get; set; }

    public virtual Consultum? IdConsultaNavigation { get; set; }

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
}
