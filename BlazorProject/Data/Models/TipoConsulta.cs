using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class TipoConsulta
{
    public int IdTipoConsulta { get; set; }

    public string Descricao { get; set; } = null!;

    public decimal? PrecoFixo { get; set; }

    public decimal? PrecoHora { get; set; }

    public int? IdUtilizador { get; set; }

    public virtual ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
}
