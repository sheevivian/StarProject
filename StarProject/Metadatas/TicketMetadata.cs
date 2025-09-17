using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{
	public class TicketMetadata
	{
		[Display(Name = "票券編號")]
		public int No { get; set; }

		[Display(Name = "票券名稱")]
		[Required(ErrorMessage = "請輸入票券名稱")]
		[StringLength(maximumLength: 30)]
		public string Name { get; set; } = null!;

		[Display(Name = "圖片")]
		public string? Image { get; set; }

		[Display(Name = "圖片")]
		public string? ImageFile { get; set; }

		public string TicCategoryNo { get; set; } = null!;

		[Display(Name = "票券類型")]
		public string Type { get; set; } = null!;

		[Display(Name = "定價")]
		[Required(ErrorMessage = "請輸入票券定價")]
		[RegularExpression(@"^\d+$", ErrorMessage = "請輸入整數數字")]
		[Range(0, 999999, ErrorMessage = "請輸入正確範圍的數字")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public decimal Price { get; set; }

		[Display(Name = "狀態")]
		[Required(ErrorMessage = "請選擇票券狀態")]
		public string Status { get; set; } = null!;

		[Display(Name = "上架日期")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Required(ErrorMessage = "請選擇票券上架日期")]
		public DateTime? ReleaseDate { get; set; }

		[Display(Name = "最後更新")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime? UpdateDate { get; set; }

		[Display(Name = "描述")]
		[StringLength(maximumLength: 50)]
		public string? Desc { get; set; }

		public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

		public virtual TicCategory TicCategoryNoNavigation { get; set; } = null!;

		public virtual ICollection<TicketStock> TicketStocks { get; set; } = new List<TicketStock>();
	}
}
