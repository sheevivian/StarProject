using System.ComponentModel;

namespace StarProject.DTOs
{
	public class LoginDTO
	{
		[DisplayName("帳號")]
		public string Account { get; set; }


		[DisplayName("密碼")]
		public string Password { get; set; }
	}
}
