using System.ComponentModel;

namespace StarProject.DTOs
{
	public class LoginDTO
	{

		/// <summary>
		/// 還沒有改變為資料庫的名稱，跟加密
		/// </summary>
		[DisplayName("帳號")]
		public string Account { get; set; }


		[DisplayName("密碼")]
		public string Password { get; set; }
	}
}
