using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;
using StarProject.Models;

namespace StarProject.ViewModels
{
	[ModelMetadataType(typeof(TicketMetadata))]

	public class TicketCreateViewModel
	{
		public string Name { get; set; } = null!;

		public IFormFile ImageFile { get; set; }

		public string TicCategoryNo { get; set; } = null!;

		public string Type { get; set; } = null!;

		public decimal Price { get; set; }

		public DateTime? ReleaseDate { get; set; }

		public string? Desc { get; set; }
	}
}
