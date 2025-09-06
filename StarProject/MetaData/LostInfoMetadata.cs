using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
	internal class LostInfoMetadata
	{
		[Display(Name = "物品名稱")]
		[Required(ErrorMessage ="請輸入物品名稱")]
		public string Name { get; set; } = null!;

		[Display(Name = "物品分類")]
		[Required(ErrorMessage = "請輸入物品分類")]
		public string Category { get; set; } = null!;

		[Display(Name = "物品描述")]
		[Required(ErrorMessage = "請輸入物品描述")]
		public string Desc { get; set; } = null!;

		[Display(Name = "物品圖片")]
		public string? Image { get; set; }

		[Display(Name = "物品圖片")]
		[Required(ErrorMessage = "請選擇一張照片")]
		public IFormFile ImageFile { get; set; } = null!;

		[Display(Name = "領取狀態")]
		[Required(ErrorMessage = "請選擇物品領取狀態")]
		public string Status { get; set; } = null!;

		[Display(Name = "拾獲日期")]
		[Required(ErrorMessage = "請選擇拾獲日期")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime FoundDate { get; set; }

		[Display(Name = "發布日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]

		public DateTime CreatedDate { get; set; }

		[Display(Name = "領取人姓名")]
		public string? OwnerName { get; set; }

		[Display(Name = "領取人電話")]
		public string? OwnerPhone { get; set; }
	}
}