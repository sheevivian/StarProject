using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class TicketStockSumViewModel
	{
		[Display(Name = "票券編號")]
		public int TicketNo { get; set; }

		[Display(Name = "庫存數量")]
		public int TotalStock { get; set; }

		[Display(Name = "日期")]
		public DateTime Date { get; set; }
	}
}
