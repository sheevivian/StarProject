using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{
	public class ProCategoryMetadata
	{
		[Display(Name = "商品分類")]
		public string Name { get; set; } = null!;
	}
}
