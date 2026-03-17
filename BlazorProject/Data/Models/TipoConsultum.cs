using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class TipoConsultum
{
    public int IdTipoConsulta { get; set; }

    public string Descricao { get; set; } = null!;

    public decimal? PrecoFixo { get; set; }

    public decimal? PrecoHora { get; set; }

    public int? IdUtilizador { get; set; }

    public virtual ICollection<Consultum> Consulta { get; set; } = new List<Consultum>();

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
}
