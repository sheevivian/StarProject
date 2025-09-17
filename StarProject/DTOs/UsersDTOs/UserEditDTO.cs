using StarProject.Models;

namespace StarProject.DTOs.UsersDTOs
{
	public class UserEditDTO
	{
		public string No { get; set; }
		public string Account { get; set; }
		public string Name { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public UsersStatus Status { get; set; }
	}
}