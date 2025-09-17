using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
    internal class NewsMetadata
    {

        [Display(Name = "文章類型")]
		[Required(ErrorMessage = "請輸入文章類型")]
        public string Category { get; set; } = null!;

        [Display(Name = "標題")]
		[Required(ErrorMessage = "請輸入文章標題")]
		public string Title { get; set; } = null!;

        [Display(Name = "文章內容")]
		[Required(ErrorMessage = "請輸入文章內容")]
		public string Content { get; set; } = null!;

        [Display(Name = "建立日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "發布日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		[Required(ErrorMessage = "請輸入正確的日期與時間")]
		public DateTime PublishDate { get; set; }
    }
}