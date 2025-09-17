namespace StarProject.ViewModels
{
	public class SearchFilterVM
	{
		public string keyword { get; set; }
		public List<string> Categories { get; set; }
		public List<string> Statuses { get; set; }
		public string DateFrom { get; set; }
		public string DateTo { get; set; }
		public int? QuantityLowerVal { get; set; }
		public int? QuantityHigherVal { get; set; }

		// ⚡ 將 internal 改成 public set，讓 JSON 可以賦值
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10; // 預設 10
	}
}