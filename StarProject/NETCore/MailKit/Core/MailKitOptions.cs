





namespace NETCore.MailKit.Core
{
	internal class MailKitOptions : Infrastructure.Internal.MailKitOptions
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public string SenderName { get; set; }
		public string SenderEmail { get; set; }
		public string Account { get; set; }
		public string Password { get; set; }
		public bool Security { get; set; }
	}
}