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

    public virtual TicCategory TicCategoryNoNavigation { get; set; } = null!;
}
