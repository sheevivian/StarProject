using System.ComponentModel.DataAnnotations;

namespace StarProject.Metadatas
{
	public class TicCategoryMetadata
	{
		[Display(Name = "票券分類")]
		public string Name { get; set; } = null!;
	}
}
