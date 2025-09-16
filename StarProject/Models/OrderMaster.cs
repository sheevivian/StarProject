using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderMaster
{
    public string No { get; set; } = null!;

    public string UserNo { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal Total { get; set; }

    public string Deliveryway { get; set; } = null!;

    public decimal Deliveryfee { get; set; }

    public string? CouponCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal AllTotal { get; set; }

    public string MerchantTradeNo { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public string? DiscountType { get; set; }

    public virtual ICollection<OrderC> OrderCs { get; set; } = new List<OrderC>();

    public virtual ICollection<OrderDelivery> OrderDeliveries { get; set; } = new List<OrderDelivery>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User UserNoNavigation { get; set; } = null!;
}
