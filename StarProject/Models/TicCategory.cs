using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class TicCategory
{
    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
