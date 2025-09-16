using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class TicketStock
{
    public int TicketNo { get; set; }

    public DateTime Date { get; set; }

    public int Stock { get; set; }

    public int No { get; set; }

    public virtual Ticket TicketNoNavigation { get; set; } = null!;
}
