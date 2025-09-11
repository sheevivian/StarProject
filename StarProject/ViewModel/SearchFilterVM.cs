namespace StarProject.ViewModel
{
	public class SearchFilterVM
	{
		public string keyword { get; set; }
		public List<string> Categories { get; set; }
		public List<string> Statuses { get; set; }
		public string DateFrom { get; set; }
		public string DateTo { get; set; }
		public int Page { get; internal set; }
		public int PageSize { get; internal set; }
	}
}