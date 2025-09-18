using System.Collections.Generic;
using System.Linq;
using StarProject.Models;
using StarProject.DTOs.RoleDTOs;

namespace StarProject.Helpers
{
	public class RoleHelper
	{
		// 頁面權限對照表
		private static readonly Dictionary<string, string> PageMap = new Dictionary<string, string>
		{
			{ "Emp", "員工管理" },
			{ "User", "會員管理" },
			{ "Info", "資訊管理" },
			{ "Event", "活動管理" },
			{ "Pd", "商品管理" },
			{ "Tic", "門票管理" },
			{ "Pm", "優惠管理" },
			{ "Order", "訂單管理" },
			{ "Cs", "客戶服務" },
			{ "Oa", "營運分析" },
			{ "CoNlist", "公司公告" },
			{ "CoNe", "修改公司公告" }
		};

		// 職位代碼到顯示名稱的映射
		public static readonly Dictionary<string, string> RoleDisplayMap = new Dictionary<string, string>
		{
			{ "RS", "研究人員" },
			{ "EX", "策展人員" },
			{ "MK", "行銷人員" },
			{ "IT", "技術人員" },
			{ "HR", "人資人員" },
			{ "FN", "財務人員" },
			{ "VS", "服務人員" },
			{ "MG", "管理人員" }
		};

		// 部門代碼到顯示名稱的映射
		public static readonly Dictionary<string, string> DepartmentDisplayMap = new Dictionary<string, string>
		{
			{ "RS", "天文研究部" },
			{ "EX", "展覽企劃部" },
			{ "MK", "行銷推廣部" },
			{ "IT", "資訊技術部" },
			{ "HR", "人力資源部" },
			{ "FN", "財務會計部" },
			{ "VS", "導覽服務部" },
			{ "MG", "管理部門" }  // 新增 MG 部門對應
		};

		// 主要方法：獲取完整的角色顯示資訊
		public static RoleDisplayInfo GetRoleDisplayInfo(string deptCode, Role role)
		{
			// 確保 role 不是 null
			if (role == null)
			{
				return new RoleDisplayInfo
				{
					RoleCode = "UNKNOWN",
					RoleDisplayName = "未知職位",
					DepartmentCode = deptCode ?? "UNKNOWN",
					DepartmentDisplayName = GetDepartmentDisplayName(deptCode ?? "UNKNOWN"),
					IsManager = false,
					PermissionCount = 0,
					PermissionSummary = "無權限",
					AccessiblePages = new List<string>()
				};
			}

			string roleCode = role.RoleName ?? "UNKNOWN";

			return new RoleDisplayInfo
			{
				RoleCode = roleCode,
				RoleDisplayName = GetRoleDisplayName(roleCode),
				DepartmentCode = deptCode ?? "UNKNOWN",
				DepartmentDisplayName = GetDepartmentDisplayName(deptCode ?? "UNKNOWN"),
				IsManager = IsManager(roleCode),
				PermissionCount = GetPermissionCount(role),
				PermissionSummary = GetPermissionSummary(role),
				AccessiblePages = GetAccessiblePagesList(role)
			};
		}

		// 獲取職位顯示名稱
		public static string GetRoleDisplayName(string roleCode)
		{
			if (string.IsNullOrEmpty(roleCode))
				return "未知職位";

			return RoleDisplayMap.ContainsKey(roleCode)
				? RoleDisplayMap[roleCode]
				: $"職位代碼: {roleCode}";
		}

		// 獲取部門顯示名稱
		public static string GetDepartmentDisplayName(string deptCode)
		{
			if (string.IsNullOrEmpty(deptCode))
				return "未知部門";

			return DepartmentDisplayMap.ContainsKey(deptCode)
				? DepartmentDisplayMap[deptCode]
				: $"部門代碼: {deptCode}";
		}

		// 判斷是否為管理人員
		public static bool IsManager(string roleCode)
		{
			return roleCode == "MG";
		}

		// 獲取權限列表
		public static List<string> GetAccessiblePagesList(Role role)
		{
			var accessiblePages = new List<string>();

			if (role == null) return accessiblePages;

			// 使用 reflection 檢查權限
			foreach (var property in typeof(Role).GetProperties())
			{
				if (PageMap.ContainsKey(property.Name))
				{
					if (property.PropertyType == typeof(bool))
					{
						bool isEnabled = (bool)property.GetValue(role);
						if (isEnabled)
						{
							accessiblePages.Add(PageMap[property.Name]);
						}
					}
				}
			}

			return accessiblePages;
		}

		// 獲取權限數量
		public static int GetPermissionCount(Role role)
		{
			return GetAccessiblePagesList(role).Count;
		}

		// 獲取權限摘要
		public static string GetPermissionSummary(Role role, int maxShow = 2)
		{
			var permissions = GetAccessiblePagesList(role);
			if (permissions.Count == 0)
				return "無權限";

			if (permissions.Count <= maxShow)
			{
				return string.Join("、", permissions);
			}
			else
			{
				var shown = permissions.Take(maxShow).ToList();
				return $"{string.Join("、", shown)} 等{permissions.Count}項功能";
			}
		}
	}
}