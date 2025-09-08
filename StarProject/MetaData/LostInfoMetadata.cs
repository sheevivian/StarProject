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
		public IFormFile? ImageFile { get; set; }

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
		[RequiredIf("Status", "已領取", ErrorMessage = "領取者姓名必填")]
		public string? OwnerName { get; set; }

		[Display(Name = "領取人手機號碼")]
		[RequiredIf("Status", "已領取", ErrorMessage = "領取者手機號碼必填")]
		[StringLength(10, ErrorMessage = "請輸入正確聯絡電話")]
		[RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的台灣手機號碼，例如 0912345678")]
		public string? OwnerPhone { get; set; }
	}
}