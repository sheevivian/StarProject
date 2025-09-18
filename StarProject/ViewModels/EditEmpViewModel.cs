using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StarProject.ViewModels
{
	public class EditEmpViewModel
	{
		public string No { get; set; }

		[Required(ErrorMessage = "員工姓名為必填")]
		[StringLength(50, ErrorMessage = "員工姓名長度不能超過50個字元")]
		public string Name { get; set; }

		[Required(ErrorMessage = "請選擇部門")]
		[Range(1, int.MaxValue, ErrorMessage = "請選擇有效的部門")]
		public int DeptNo { get; set; }

		[Required(ErrorMessage = "請選擇職位")]
		[Range(1, int.MaxValue, ErrorMessage = "請選擇有效的職位")]
		public int RoleNo { get; set; }

		[Required(ErrorMessage = "請選擇到職日期")]
		[DataType(DataType.Date)]
		public DateTime HireDate { get; set; }

		[Required(ErrorMessage = "Email為必填")]
		[EmailAddress(ErrorMessage = "Email格式不正確")]
		public string Email { get; set; }

		[Required(ErrorMessage = "電話為必填")]
		[Phone(ErrorMessage = "電話格式不正確")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "身分證字號為必填")]
		[StringLength(10, MinimumLength = 10, ErrorMessage = "身分證字號必須為10碼")]
		public string IdNumber { get; set; }

		[Required(ErrorMessage = "請選擇生日")]
		[DataType(DataType.Date)]
		public DateTime BirthDate { get; set; }

		public bool Status { get; set; }

		// 下拉選單資料 (這些是OK的，因為它們是用於建構View)
		public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> Depts { get; set; } = new List<SelectListItem>();
	}
}