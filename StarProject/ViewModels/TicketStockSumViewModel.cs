using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class TicketStockSumViewModel
	{
		[Display(Name = "票券編號")]
		public int TicNo { get; set; }

		[Display(Name = "票券名稱")]
		public string TicName { get; set; }

		[Display(Name = "圖片")]
		public string TicImg { get; set; }

		[Display(Name = "票券種類")]
		public string TicCategory { get; set; }

		[Display(Name = "票券類型")]
		public string TicType { get; set; }

		[Display(Name = "定價")]
		public string TicPrice { get; set; }

		[Display(Name = "票券狀態")]
		public string TicStatus { get; set; }

		[Display(Name = "描述")]
		public string TicDesc { get; set; }

		[Display(Name = "庫存數量")]
		public int SumQuantity { get; set; }

		[Display(Name = "最後異動時間")]
		public DateTime UpdateDate { get; set; }
	}
}
