namespace StarProject.DTOs.EmpsDTOs
{
	public class EmpsDTO
	{
		public string No { get; set; }               // 主鍵 GUID
		public string EmpCode { get; set; }          // 員工編號
		public string Name { get; set; }             // 員工姓名
		public int DeptNo { get; set; }              // 部門編號
		public string DeptName { get; set; }         // 部門名稱（用於顯示）
		public int RoleNo { get; set; }              // 角色編號
		public string RoleName { get; set; }         // 角色名稱（用於顯示）
		public DateTime HireDate { get; set; }       // 入職日期
		public bool Status { get; set; }             // 啟用狀態
		public bool ForceChangePassword { get; set; } // 是否強制修改密碼
	}
}
