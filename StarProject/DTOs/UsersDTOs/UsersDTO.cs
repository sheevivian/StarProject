using Microsoft.Build.Framework;
using StarProject.Models;
using System.ComponentModel.DataAnnotations;
using StarProject.Helpers;
using RequiredAttribute = Microsoft.Build.Framework.RequiredAttribute;
namespace StarProject.DTOs.UsersDTOs
{

	public class UsersDTO
	{

		public string Id { get; set; }               // 資料庫主鍵，用於連結
		public string Account { get; set; }

		public string Name { get; set; }

		public string Phone { get; set; }

		public string Email { get; set; }

		public string Address { get; set; }

		public UsersStatus Status { get; set; }     // 存 Enum
		public string StatusText { get; set; }       // 顯示中文

	}
}

