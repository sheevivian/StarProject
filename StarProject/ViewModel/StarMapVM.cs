using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using StarProject.MetaData;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModel
{
	[ModelMetadataType(typeof(StarMapMetadata))]
	public class StarMapVM
	{
		public int No { get; set; }
		public string Name { get; set; } = null!;

		public string Desc { get; set; } = null!;

		public IFormFile? ImageFile { get; set; }

		public string? Image { get; set; }

        public string Address { get; set; } = null!;

        public decimal MapLatitude { get; set; }

        public decimal MapLongitude { get; set; }
    }
}
