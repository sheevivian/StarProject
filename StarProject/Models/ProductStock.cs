using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProductStock
{
    public int ProductNo { get; set; }

    public string Type { get; set; } = null!;

    public int TransQuantity { get; set; }

    public DateTime Date { get; set; }

    public string? Note { get; set; }

    public int No { get; set; }

    public virtual Product ProductNoNavigation { get; set; } = null!;
}
