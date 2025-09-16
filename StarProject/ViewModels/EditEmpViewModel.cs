using System;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
	public class EditEmpViewModel
	{
		[Required]
		public string No { get; set; }

		[Required(ErrorMessage = "姓名為必填欄位")]
		[StringLength(50, ErrorMessage = "姓名長度不能超過50個字元")]
		public string Name { get; set; }

		[Required(ErrorMessage = "請選擇角色")]
		[Display(Name = "角色")]
		public int RoleNo { get; set; }

		[Required(ErrorMessage = "請選擇部門")]
		[Display(Name = "部門")]
		public int DeptNo { get; set; }

		[Required(ErrorMessage = "到職日期為必填欄位")]
		[Display(Name = "到職日期")]
		[DataType(DataType.Date)]
		public DateTime HireDate { get; set; }

		[Display(Name = "狀態")]
		public bool Status { get; set; }

		[EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
		[Display(Name = "電子郵件")]
		public string Email { get; set; }

		[Phone(ErrorMessage = "請輸入有效的電話號碼")]
		[Display(Name = "電話")]
		public string Phone { get; set; }

		[Display(Name = "身分證字號")]
		[StringLength(10, ErrorMessage = "身分證字號長度應為10碼")]
		public string IdNumber { get; set; }

		[Display(Name = "生日")]
		[DataType(DataType.Date)]
		public DateTime? BirthDate { get; set; }
	}
}