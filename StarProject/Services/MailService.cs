using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using StarProject.Models;
using System.Threading.Tasks;

namespace StarProject.Services
{
	public class MailService
	{
		private readonly IConfiguration _config;
		public MailService(IConfiguration config) => _config = config;

		public async Task SendEmailAsync(MailMessageModel message)
		{
			var emailSettings = _config.GetSection("EmailSettings");

			var email = new MimeMessage();
			email.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
			email.To.Add(MailboxAddress.Parse(message.To));
			email.Subject = message.Subject;
			email.Body = new TextPart("html") { Text = message.Body };

			using var client = new SmtpClient();
			await client.ConnectAsync(
				emailSettings["SmtpServer"],
				int.Parse(emailSettings["Port"]),
				MailKit.Security.SecureSocketOptions.StartTls
			);
			await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
			await client.SendAsync(email);
			await client.DisconnectAsync(true);
		}

		// 報名成功通知
		public async Task SendRegistrationSuccessEmail(string to, string eventName, DateTime eventDate)
		{
			var message = new MailMessageModel
			{
				To = to,
				Subject = $"【{eventName}】報名成功通知",
				Body = $"<p>您好，您已成功報名活動：<strong>{eventName}</strong></p>" +
					   $"<p>活動日期：{eventDate:yyyy-MM-dd}</p>" +
					   "<p>期待您的參與！</p>"+
					   "<p>阿波羅天文館</p>"
			};
			await SendEmailAsync(message);
		}

		// 活動前提醒
		public async Task SendEventReminderEmail(string to, string eventName, DateTime eventDate)
		{
			var message = new MailMessageModel
			{
				To = to,
				Subject = $"【{eventName}】活動即將開始提醒",
				Body = $"<p>您好，提醒您參加的活動 <strong>{eventName}</strong> 即將開始</p>" +
					   $"<p>活動日期：{eventDate:yyyy-MM-dd}</p>" +
					   "<p>請準時於本館大廳報到，期待與您相見！</p>" +
					   "<p>阿波羅天文館</p>"
			};
			await SendEmailAsync(message);
		}
	}
}
