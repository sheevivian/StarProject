namespace StarProject.DTOs.EmpsDTOs
{
	public class SearchEmpRequest
	{
		public string Keyword { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public List<string> Departments { get; set; } = new List<string>();
		public List<string> Roles { get; set; } = new List<string>();
		public List<string> Statuses { get; set; } = new List<string>();
		public string DateFrom { get; set; }
		public string DateTo { get; set; }
	}


}
