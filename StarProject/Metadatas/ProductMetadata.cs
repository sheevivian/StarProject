using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{

	public class ProductMetadata
	{
		[Display(Name = "商品編號")]
		public int No { get; set; } 

		[Display(Name = "商品名稱")]
		[Required(ErrorMessage = "請輸入商品名稱")]
		[StringLength(maximumLength: 30)]
		public string Name { get; set; } = null!;

		[Display(Name = "定價")]
		[Required(ErrorMessage = "請輸入商品定價")]
		[RegularExpression(@"^\d+$", ErrorMessage = "請輸入整數數字")]
		[Range(0, 999999, ErrorMessage = "請輸入正確範圍的數字")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public decimal Price { get; set; }

		[Display(Name = "商品狀態")]
		[Required(ErrorMessage = "請選擇商品狀態")]
		public string Status { get; set; } = null!;

		[Display(Name = "上架日期")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Required(ErrorMessage = "請選擇商品上架日期")]
		public DateTime? ReleaseDate { get; set; }

		[Display(Name = "最後更新")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime? UpdateDate { get; set; }

	}
}
