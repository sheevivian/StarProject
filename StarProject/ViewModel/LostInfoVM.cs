using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarProject.MetaData;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModel
{
	[ModelMetadataType(typeof(LostInfoMetadata))]
	public class LostInfoVM
	{
		public string Name { get; set; } = null!;

		public string Category { get; set; } = null!;

		public string Desc { get; set; } = null!;

		public IFormFile ImageFile { get; set; } = null!;
		public string? Image { get; set; }

		public string Status { get; set; } = null!;

		public DateTime FoundDate { get; set; }

		public DateTime CreatedDate { get; set; }

		public string? OwnerName { get; set; }

		public string? OwnerPhone { get; set; }
	}
}
