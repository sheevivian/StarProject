using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Cart
{
    public int No { get; set; }

    public string UserNo { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal Total { get; set; }

    public decimal AllTotal { get; set; }

    public DateTime Date { get; set; }

    public string? CouponCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime UpdatedAtTime { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User UserNoNavigation { get; set; } = null!;
}
