using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{
	public class ProIntroMetadata
	{
		[Display(Name = "商品編號")]
		public int ProductNo { get; set; }

		[Display(Name = "商品名稱")]
		public string ProductName { get; set; }

		[Display(Name = "商品賣點")]
		[StringLength(maximumLength: 50)]
		public string? Point { get; set; }

		[Display(Name = "商品介紹")]
		public string? Description { get; set; }

	}
}
