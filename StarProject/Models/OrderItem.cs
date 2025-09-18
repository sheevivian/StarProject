using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderItem
{
    public int ListId { get; set; }

    public string OrderNo { get; set; } = null!;

    public int? ProductNo { get; set; }

    public int? TicketNo { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Image { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal? Discount { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public string? CouponCode { get; set; }

    public string? Type { get; set; }

    public string? DiscountType { get; set; }

    public virtual OrderMaster OrderNoNavigation { get; set; } = null!;

    public virtual Ticket? TicketNoNavigation { get; set; }
}
