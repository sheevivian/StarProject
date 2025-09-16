using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Ticket
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public string TicCategoryNo { get; set; } = null!;

    public string Type { get; set; } = null!;

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ReleaseDate { get; set; }

    public string? Desc { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual TicCategory TicCategoryNoNavigation { get; set; } = null!;

    public virtual ICollection<TicketStock> TicketStocks { get; set; } = new List<TicketStock>();

    public virtual ICollection<TicketTransStock> TicketTransStocks { get; set; } = new List<TicketTransStock>();
}
