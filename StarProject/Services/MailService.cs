using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using StarProject.Models;
using System.Threading.Tasks;
using System.Net; // 用於 HtmlEncode
using QRCoder;
using System.IO;

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

		// 產生 QR 圖（PNG bytes）
		private static byte[] GenerateQrPng(string text)
		{
			using var generator = new QRCodeGenerator();
			using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			var png = new PngByteQRCode(data);
			return png.GetGraphic(20);
		}

		// 報名成功通知內容
		public async Task SendRegistrationSuccessEmail(
			string to,
			string eventName,
			DateTime eventTime,
			string? qrPayload = null,
			string? recipientName = null
		)
		{
			var emailSettings = _config.GetSection("EmailSettings");

			qrPayload ??= $"SP|EV={eventName}|DT={eventTime:yyyyMMddHHmm}|K={Guid.NewGuid():N}";
			var qrBytes = GenerateQrPng(qrPayload);

			var namePart = !string.IsNullOrWhiteSpace(recipientName)
				? WebUtility.HtmlEncode(recipientName.Trim()) + " "
				: string.Empty;
			var greeting = $"{namePart}星際旅伴 您好";
			var safeEventName = WebUtility.HtmlEncode(eventName);

			var msg = new MimeMessage();
			msg.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
			msg.To.Add(MailboxAddress.Parse(to));
			msg.Subject = $"【{eventName}】報名成功通知";

			var cid = "qr1";
			var builder = new BodyBuilder
			{
				HtmlBody =
					$"<p>{greeting}，您已成功報名活動：<strong>{safeEventName}</strong></p>" +
					$"<p>活動時間為 {eventTime:yyyy-MM-dd HH:mm}</p>" +
					$"<p>這是您的入場 QR Code（請於報到時出示）：</p>" +
					$"<p><img alt=\"QR Code\" src=\"cid:{cid}\" style=\"max-width:240px;\"/></p>" +
					"<p>期待您的參與！</p>" +
					"<p>阿波羅天文館</p>"
			};

			var image = new MimePart("image", "png")
			{
				Content = new MimeContent(new MemoryStream(qrBytes)),
				ContentId = cid,
				ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
				ContentTransferEncoding = ContentEncoding.Base64,
				FileName = "qrcode.png"
			};
			builder.LinkedResources.Add(image);
			msg.Body = builder.ToMessageBody();

			// 4) 寄送
			using var client = new SmtpClient();
			await client.ConnectAsync(
				emailSettings["SmtpServer"],
				int.Parse(emailSettings["Port"]),
				MailKit.Security.SecureSocketOptions.StartTls
			);
			await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
			await client.SendAsync(msg);
			await client.DisconnectAsync(true);
		}

	}
}
