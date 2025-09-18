using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;
using StarProject.Models;

namespace StarProject.ViewModels
{
	[ModelMetadataType(typeof(ProIntroMetadata))]
	public class ProductIntroViewModel
	{
		// 基本資料
		public int ProductNo { get; set; }
		public string ProductName { get; set; }

		// 商品介紹
		public string? Description { get; set; }
		public string? Point { get; set; }

		// 商品圖片
		public List<string> Images { get; set; } = new List<string>();

	}
}
