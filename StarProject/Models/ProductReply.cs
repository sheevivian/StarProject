using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProductReply
{
    public int ProductNo { get; set; }

    public string UserNo { get; set; } = null!;

    public string? Reply { get; set; }

    public decimal Ratings { get; set; }

    public virtual Product ProductNoNavigation { get; set; } = null!;

    public virtual User UserNoNavigation { get; set; } = null!;
}
