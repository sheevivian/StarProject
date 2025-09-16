using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	[ModelMetadataType(typeof(TicketMetadata))]
	public class TicketStockSumViewModel
	{
		public int No { get; set; }

		public string Name { get; set; }

		public string Image { get; set; }

		[DisplayName("票券種類")]
		public string TicCategory { get; set; }

		public string Type { get; set; }

		public DateTime ReleaseDate { get; set; }

		[DisplayName("庫存")]
		public int TotalStock { get; set; }
	}
}
