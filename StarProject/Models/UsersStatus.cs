using System.ComponentModel.DataAnnotations;

namespace StarProject.Models
{
	public enum UsersStatus
	{
		[Display(Name = "停用")]
		Disabled = 0,

		[Display(Name = "正常")]
		Active = 1,

		[Display(Name = "封鎖")]
		Blocked = 2
	}
}
