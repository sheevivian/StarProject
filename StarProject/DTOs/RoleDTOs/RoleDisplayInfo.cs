namespace StarProject.DTOs.RoleDTOs
{
	public class RoleDisplayInfo
	{
		public string RoleCode { get; set; }
		public string RoleDisplayName { get; set; }
		public string DepartmentCode { get; set; }
		public string DepartmentDisplayName { get; set; }
		public bool IsManager { get; set; }
		public int PermissionCount { get; set; }
		public string PermissionSummary { get; set; }
		public List<string> AccessiblePages { get; set; }
	}
}
