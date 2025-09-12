using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StarProject.Helpers
{
	public static class UsersStatusEnumExtensions
	{
		public static string GetDisplayName(this Enum value)
		{
			var field = value.GetType().GetField(value.ToString());
			var attribute = field.GetCustomAttribute<DisplayAttribute>();
			return attribute?.Name ?? value.ToString();
		}
	}
}
