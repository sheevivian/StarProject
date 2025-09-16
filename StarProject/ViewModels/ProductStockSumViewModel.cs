using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class ProductStockSumViewModel
	{
		[Display(Name="商品編號")]
		public int ProductNo { get; set; }

		[Display(Name = "商品名稱")]
		public string ProductName { get; set; }

		[Display(Name = "商品分類")]
		public string ProCategoryName { get; set; }

		[Display(Name = "庫存數量")]
		public int SumQuantity { get; set; }

		[Display(Name = "最後異動時間")]
		public DateTime UpdateDate { get; set; }

	}
}
