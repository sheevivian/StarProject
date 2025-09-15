using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class NewsImage
{
    public int No { get; set; }

    public int NewsNo { get; set; }

    public int OrderNo { get; set; }

    public string Image { get; set; } = null!;

    public virtual News NewsNoNavigation { get; set; } = null!;
}
