using StarProject.Models;
using System;
using System.Collections.Generic;

namespace StarProject.ViewModel
{
	public class EventListVm
	{
		public List<Event> Events { get; set; } = new();
		public string Keyword { get; set; } = string.Empty;
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 12;
		public int Total { get; set; }

		public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
	}
}
