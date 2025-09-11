using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class TickestStock
{
    public int TicketNo { get; set; }

    public DateTime Date { get; set; }

    public int Stock { get; set; }

    public virtual Ticket TicketNoNavigation { get; set; } = null!;
}
