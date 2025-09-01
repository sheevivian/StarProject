using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProductImage
{
    public int ProductNo { get; set; }

    public string Image { get; set; } = null!;

    public int? ImgOrder { get; set; }

    public virtual Product ProductNoNavigation { get; set; } = null!;
}
