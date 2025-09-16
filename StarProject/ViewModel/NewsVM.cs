using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Build.Framework;
using StarProject.MetaData;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModel
{
	[ModelMetadataType(typeof(NewsMetadata))]
	public class NewsVM
	{
        public NewsVM()
        {
            PublishDate = DateTime.Now; // 預設今天
        }
        public int No { get; set; }
        public string Category { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime PublishDate { get; set; }

		// 舊圖片相關
		public List<string> Images { get; set; } = new();       // 舊圖片 URL
		public List<int> ImageIds { get; set; } = new();        // 舊圖片 Id (NewsImage.No)

		[ValidateNever]
		public Dictionary<int, int> ImageOrderMap { get; set; } = new();
		// key=舊圖片 Id, value=順序

		[ValidateNever]
		public List<int> DeleteImageIds { get; set; } = new(); // 待刪除的舊圖片 Id

		// 新圖片相關
		public List<IFormFile> ImageFiles { get; set; } = new(); // 上傳新圖片
		[ValidateNever]
		public List<int> ImageOrderNos { get; set; } = new();   // 新圖片順序
	}
}
