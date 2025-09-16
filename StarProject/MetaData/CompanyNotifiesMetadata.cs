using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
	internal class CompanyNotifyMetadata
	{

		[Display(Name = "標題")]
		[Required(ErrorMessage = "請輸入標題")]
        [StringLength(50, ErrorMessage = "標題過長，請修正")]
        public string Title { get; set; } = null!;

		[Display(Name = "內容")]
		public string? Content { get; set; }

		[Display(Name = "公告類型")]
		[Required(ErrorMessage = "請輸入標題")]
		public string Category { get; set; } = null!;

		[Display(Name = "發布日期")]

		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		public DateTime PublishDate { get; set; }
	}
}