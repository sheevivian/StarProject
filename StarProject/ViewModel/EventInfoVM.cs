using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using StarProject.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;


namespace StarProject.ViewModel
{
	[ModelMetadataType(typeof(EventMetadata))]
	public class EventInfoVM
	{
		public int No { get; set; }
		public string Title { get; set; }
		public string? Desc { get; set; }
		public string Category { get; set; }
		public string Location { get; set; }
		public IFormFile ImageFile { get; set; } // 上傳用
		public string? Image { get; set; }        // 原圖 URL
		public DateTime StartDate { get; set; }
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
		public DateTime? EndDate { get; set; }
		public DateTime CreatedTime { get; set; }
		public DateTime UpdatedTime { get; set; }
		public int MaxParticipants { get; set; }
		public string Status { get; set; }
		public int? Fee { get; set; }
		public int? Deposit { get; set; }
	}
}

