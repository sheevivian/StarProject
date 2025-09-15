namespace StarProject.ViewModels
{
	public class EventCardVm
	{
		public int No { get; set; }
		public string Category { get; set; } = "";
		public string Title { get; set; } = "";
		public System.DateTime StartDate { get; set; }
		public string Location { get; set; } = "";
		public string? CoverImageUrl { get; set; }
		public int MaxParticipants { get; set; }
		public int CurrentCount { get; set; }
	}
}
