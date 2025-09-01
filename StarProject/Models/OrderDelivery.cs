using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderDelivery
{
    public string OrderNo { get; set; } = null!;

    public string UserNo { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual OrderMaster OrderNoNavigation { get; set; } = null!;

    public virtual User UserNoNavigation { get; set; } = null!;
}
