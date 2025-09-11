using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{

	public class ProductMetadata
	{
		[Display(Name = "商品編號")]
		public int No { get; set; } 

		[Display(Name = "商品名稱")]
		public string Name { get; set; } = null!;

		[Display(Name = "定價")]
		public decimal Price { get; set; }

		[Display(Name = "狀態")]
		public string Status { get; set; } = null!;

		[Display(Name = "上架日期")]
		public DateTime? ReleaseDate { get; set; }

		[Display(Name = "最後更新")]
		public DateTime? UpdateDate { get; set; }

	}
}
