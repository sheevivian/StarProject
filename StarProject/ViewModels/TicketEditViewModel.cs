using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;

namespace StarProject.ViewModels
{
	[ModelMetadataType(typeof(TicketMetadata))]

	public class TicketEditViewModel
	{
		public int No { get; set; }

		public string Name { get; set; } = null!;

		public string Image { get; set; }

		public IFormFile ImageFile { get; set; }

		public string TicCategoryNo { get; set; } = null!;

		public string Type { get; set; } = null!;

		public decimal Price { get; set; }

		public string Status { get; set; }

		public DateTime? ReleaseDate { get; set; }

		public string? Desc { get; set; }
	}
}
