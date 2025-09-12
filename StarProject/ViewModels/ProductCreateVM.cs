using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;
using StarProject.Models;

namespace StarProject.ViewModels
{
	[ModelMetadataType(typeof(ProductMetadata))]
	public class ProductCreateVM
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

	}
}
