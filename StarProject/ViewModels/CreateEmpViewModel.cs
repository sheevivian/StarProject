using System;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class CreateEmpViewModel
	{
		[Required(ErrorMessage = "姓名為必填欄位")]
		[StringLength(50, ErrorMessage = "姓名不可超過50個字元")]
		[Display(Name = "姓名")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "部門為必填欄位")]
		[Display(Name = "部門")]
		public int DeptNo { get; set; }

		[Required(ErrorMessage = "職位為必填欄位")]
		[Display(Name = "職位")]
		public int RoleNo { get; set; }

		[Required(ErrorMessage = "到職日期為必填欄位")]
		[DataType(DataType.Date)]
		[Display(Name = "到職日期")]
		public DateTime HireDate { get; set; } = DateTime.Today;

		[EmailAddress(ErrorMessage = "請輸入有效的Email格式")]
		[StringLength(100, ErrorMessage = "Email不可超過100個字元")]
		[Display(Name = "Email")]
		public string? Email { get; set; }

		[StringLength(50, ErrorMessage = "電話不可超過50個字元")]
		[Display(Name = "電話")]
		public string? Phone { get; set; }

		[StringLength(10, MinimumLength = 10, ErrorMessage = "身分證字號必須為10個字元")]
		[RegularExpression(@"^[A-Z][1-2]\d{8}$", ErrorMessage = "請輸入正確的身分證字號格式")]
		[Display(Name = "身分證字號")]
		public string? IdNumber { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "生日")]
		public DateTime? BirthDate { get; set; }
	}
}