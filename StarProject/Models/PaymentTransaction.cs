using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class PaymentTransaction
{
    public int No { get; set; }

    public string MerchantTradeNo { get; set; } = null!;

    public string SourceType { get; set; } = null!;

    public string SourceId { get; set; } = null!;

    public string PaymentProvider { get; set; } = null!;

    public string PaymentWay { get; set; } = null!;

    public string? ProviderTransId { get; set; }

    public string Status { get; set; } = null!;

    public decimal PaidAmount { get; set; }

    public DateTime? PaidTime { get; set; }

    public string? RawResponse { get; set; }

    public DateTime CreatedAt { get; set; }
}
