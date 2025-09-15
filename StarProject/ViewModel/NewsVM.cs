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
		public int No { get; set; }
        public string Category { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime PublishDate { get; set; }

        // 多張圖片
        public List<string> Images { get; set; } = new();

        // 上傳的新圖片 (接收 <input type="file" multiple /> )
        public List<IFormFile> ImageFiles { get; set; } = new();

		[ValidateNever]
		// 新增 OrderNo 清單（對應每張 ImageFiles 的順序）
		public List<int> ImageOrderNos { get; set; } = new();
	}
}
