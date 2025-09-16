using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Product
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public string ProCategoryNo { get; set; } = null!;

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ReleaseDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ProCategory ProCategoryNoNavigation { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
}
