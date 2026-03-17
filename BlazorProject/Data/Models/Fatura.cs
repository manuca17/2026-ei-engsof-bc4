using System;
using System.Collections.Generic;

namespace BlazorProject.Data.Models;

public partial class Fatura
{
    public int IdFatura { get; set; }

    public decimal? ValorPago { get; set; }

    public DateTime? DhPagamento { get; set; }

    public string? Metodo { get; set; }

    public virtual ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();
}
