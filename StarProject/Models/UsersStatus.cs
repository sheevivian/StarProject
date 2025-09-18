using System.ComponentModel.DataAnnotations;

namespace StarProject.Models
{
	public enum UsersStatus : byte
	{
		[Display(Name = "正常")]
		Normal = 1,

		[Display(Name = "停用")]
		Suspended = 2,

		[Display(Name = "封鎖")]
		Blocked = 3
	}
}