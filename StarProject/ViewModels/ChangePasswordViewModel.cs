using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "請輸入目前密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "目前密碼")]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入新密碼")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須至少6個字元")]
		[DataType(DataType.Password)]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
			ErrorMessage = "密碼必須包含大小寫字母和數字")]
		[Display(Name = "新密碼")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "請確認新密碼")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不相符")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; } = string.Empty;

		public bool IsForced { get; set; } = false; // 是否為強制修改
	}
}
