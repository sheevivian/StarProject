using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class PromotionUsage
{
    public int PromotionNo { get; set; }

    public string UserNo { get; set; } = null!;

    public DateTime UsedDate { get; set; }

    public virtual Promotion PromotionNoNavigation { get; set; } = null!;

    public virtual User UserNoNavigation { get; set; } = null!;
}
