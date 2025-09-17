using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderDelivery
{
    public int DeliveryId { get; set; }

    public string OrderNo { get; set; } = null!;

    public string UserNo { get; set; } = null!;

    public string RecipientName { get; set; } = null!;

    public string RecipientAddress { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual OrderMaster OrderNoNavigation { get; set; } = null!;

    public virtual ICollection<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();

    public virtual User UserNoNavigation { get; set; } = null!;
}
