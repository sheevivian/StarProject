using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using QRCoder;
using StarProject.Models;
using System.Net;

namespace StarProject.Services
{
	public class EmailSettings
	{
		public string Server { get; set; } = string.Empty;
		public int Port { get; set; }
		public string SenderName { get; set; } = string.Empty;
		public string SenderEmail { get; set; } = string.Empty;
		public string Account { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool Security { get; set; }
	}

	public interface IEmailService
	{
		Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
		Task SendEmailAsync(MailMessageModel message);
		Task SendWelcomeEmailAsync(string toEmail, string empName, string empCode, string password, DateTime hireDate);
		Task SendRegistrationSuccessEmail(string to, string eventName, DateTime eventTime, string? qrPayload = null, string? recipientName = null);
	}

	public class EmailService : IEmailService
	{
		private readonly EmailSettings _emailSettings;

		public EmailService(IOptions<EmailSettings> emailSettings)
		{
			_emailSettings = emailSettings.Value;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
		{
			try
			{
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
				message.To.Add(new MailboxAddress("", toEmail));
				message.Subject = subject;

				var bodyBuilder = new BodyBuilder();
				if (isHtml)
				{
					bodyBuilder.HtmlBody = body;
				}
				else
				{
					bodyBuilder.TextBody = body;
				}
				message.Body = bodyBuilder.ToMessageBody();

				await SendMessageAsync(message);
			}
			catch (Exception ex)
			{
				throw new Exception($"Email發送失敗：{ex.Message}", ex);
			}
		}

		public async Task SendEmailAsync(MailMessageModel message)
		{
			try
			{
				var email = new MimeMessage();
				email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
				email.To.Add(MailboxAddress.Parse(message.To));
				email.Subject = message.Subject;
				email.Body = new TextPart("html") { Text = message.Body };

				await SendMessageAsync(email);
			}
			catch (Exception ex)
			{
				throw new Exception($"Email發送失敗：{ex.Message}", ex);
			}
		}

		public async Task SendWelcomeEmailAsync(string toEmail, string empName, string empCode, string password, DateTime hireDate)
		{
			string htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                    <h2 style='color: #333; margin: 0 0 10px 0;'>歡迎加入阿波羅天文館</h2>
                </div>

                <div style='background-color: white; padding: 20px; border: 1px solid #dee2e6; border-radius: 8px;'>
                    <p style='margin: 0 0 15px 0; font-size: 16px;'>親愛的 <strong>{empName}</strong> 您好：</p>
    
                    <p style='margin: 0 0 15px 0; line-height: 1.6;'>
                        恭喜您正式成為阿波羅天文館的一員！以下是您的帳號相關資訊，請妥善保管：
                    </p>
    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0 0 10px 0;'><strong>員工編號：</strong>{empCode}</p>
                        <p style='margin: 0 0 10px 0;'><strong>預設密碼：</strong>{password}</p>
                        <p style='margin: 0;'><strong>到職日期：</strong>{hireDate:yyyy年MM月dd日}</p>
                    </div>
    
                    <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin: 0 0 10px 0; color: #856404;'>⚠️ 重要提醒</h4>
                        <ul style='margin: 0; padding-left: 20px; line-height: 1.6; color: #856404;'>
                            <li>請於首次登入後立即修改密碼</li>
                            <li>密碼應包含大小寫字母、數字，長度至少8位</li>
                            <li>請勿與他人分享您的帳號密碼</li>
                        </ul>
                    </div>
    
                    <p style='margin: 20px 0 15px 0; line-height: 1.6;'>
                        如有任何問題，請隨時與人事部門聯繫。再次歡迎您的加入，期待與您一同探索星空的奧秘！
                    </p>
    
                    <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;'>
                        <p style='margin: 0; color: #6c757d; font-size: 14px;'>
                            此信件為系統自動發送，請勿直接回覆。<br>
                            © 2025 阿波羅天文館人事部門
                        </p>
                    </div>
                </div>
            </div>";

			await SendEmailAsync(toEmail, "歡迎加入阿波羅天文館 - 帳號資訊通知", htmlBody, true);
		}

		public async Task SendRegistrationSuccessEmail(string to, string eventName, DateTime eventTime, string? qrPayload = null, string? recipientName = null)
		{
			try
			{
				qrPayload ??= $"SP|EV={eventName}|DT={eventTime:yyyyMMddHHmm}|K={Guid.NewGuid():N}";
				var qrBytes = GenerateQrPng(qrPayload);

				var namePart = !string.IsNullOrWhiteSpace(recipientName)
					? WebUtility.HtmlEncode(recipientName.Trim()) + " "
					: string.Empty;
				var greeting = $"{namePart}星際旅伴 您好";
				var safeEventName = WebUtility.HtmlEncode(eventName);

				var msg = new MimeMessage();
				msg.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
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

				await SendMessageAsync(msg);
			}
			catch (Exception ex)
			{
				throw new Exception($"報名成功通知Email發送失敗：{ex.Message}", ex);
			}
		}

		// 統一的寄信邏輯
		private async Task SendMessageAsync(MimeMessage message)
		{
			using var client = new SmtpClient();
			await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port,
				_emailSettings.Security ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
			await client.AuthenticateAsync(_emailSettings.Account, _emailSettings.Password);
			await client.SendAsync(message);
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
	}
}