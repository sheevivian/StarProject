using StarProject.Models;

namespace StarProject.ViewModels
{
	public class ProductEditViewModel
	{
		public Product Product { get; set; }
		public ProductImage ProImage { get; set; } = new ProductImage();
		public List<ProductImage> ProImages { get; set; }
		public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
		public List<int> ImageOrders { get; set; }

	}
}
