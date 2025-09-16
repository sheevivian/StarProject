using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class TicketTransStock
{
    public int No { get; set; }

    public int TicketNo { get; set; }

    public string Type { get; set; } = null!;

    public int TransQuantity { get; set; }

    public DateTime Date { get; set; }

    public string? Note { get; set; }

    public virtual Ticket TicketNoNavigation { get; set; } = null!;
}
