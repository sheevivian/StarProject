using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
    internal class NewsMetadata
    {

        [Display(Name = "文章類型")]
        [Required]
        public string Category { get; set; } = null!;

        [Display(Name = "標題")]
		[Required]
		public string Title { get; set; } = null!;

        [Display(Name = "文章內容")]
		[Required]
		public string Content { get; set; } = null!;

        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "發布日期")]
		[Required]
		public DateTime PublishDate { get; set; }
    }
}