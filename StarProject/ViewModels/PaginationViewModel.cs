namespace StarProject.ViewModels
{
	public class PaginationViewModel
	{
		public int TotalRecords { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }

		// 這是為了在 View 中方便計算總頁數而新增的屬性
		public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
	}
}