using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int CartNo { get; set; }

    public int ProductNo { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? Discount { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public string? CouponCode { get; set; }

    public virtual Cart CartNoNavigation { get; set; } = null!;

    public virtual Product ProductNoNavigation { get; set; } = null!;
}
