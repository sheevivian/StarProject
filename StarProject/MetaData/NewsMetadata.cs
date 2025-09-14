using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
    internal class NewsMetadata
    {

        [Display(Name = "文章類型")]
        public string Category { get; set; } = null!;

        [Display(Name = "標題")]
        public string Title { get; set; } = null!;

        [Display(Name = "文章內容")]
        public string Content { get; set; } = null!;

        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "發布日期")]
        public DateTime PublishDate { get; set; }

        // 🔗 一筆 News 會有多張圖片
        [Display(Name = "封面照片")]
        public virtual ICollection<NewsImage> NewsImages { get; set; } = new List<NewsImage>();
    }
}