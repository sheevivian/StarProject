using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{
	public class ProStockMetadata
	{
		[Display(Name = "商品編號")]
		public int ProductNo { get; set; }

		[Display(Name = "異動類別")]
		public string Type { get; set; } = null!;

		[Display(Name = "異動數量")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public int TransQuantity { get; set; }

		[Display(Name = "更新日期")]
		public DateTime Date { get; set; }

		[Display(Name = "補充說明")]
		public string? Note { get; set; }

		public int No { get; set; }
	}
}
