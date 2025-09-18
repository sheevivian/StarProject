namespace StarProject.DTOs.UsersDTOs
{
	public class UsersFilterDTO
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? Keyword { get; set; }
		public List<string>? Categories { get; set; } = new List<string>();
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
	}
}