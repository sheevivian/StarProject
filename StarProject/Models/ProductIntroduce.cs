using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProductIntroduce
{
    public int ProductNo { get; set; }

    public string? Point { get; set; }

    public string? Description { get; set; }

    public virtual Product ProductNoNavigation { get; set; } = null!;
}
