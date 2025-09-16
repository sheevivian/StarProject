using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderStatus
{
    public int StatusId { get; set; }

    public int DeliveryId { get; set; }

    public string StatusType { get; set; } = null!;

    public DateTime StatusTime { get; set; }

    public string? Notes { get; set; }

    public virtual OrderDelivery Delivery { get; set; } = null!;
}
