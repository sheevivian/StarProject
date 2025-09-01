using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProCategory
{
    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
