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
				var greeting = $"星際旅伴{namePart}您好";
				var safeEventName = WebUtility.HtmlEncode(eventName);

				var msg = new MimeMessage();
				msg.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
				msg.To.Add(MailboxAddress.Parse(to));
				msg.Subject = $"【{eventName}】報名成功通知";

				var cid = "qr1";
				// TODO: 改成你的客服資訊（也可改成從組態讀取）
				var servicePhone = "02-1234-5678";
				var serviceEmail = "support@apollo.example";

				var builder = new BodyBuilder
				{
					HtmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""zh-Hant"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>報名成功通知</title>
                    </head>
                    <body style=""margin:0;padding:0;background:#f3f4f6;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Noto Sans TC','PingFang TC','Microsoft JhengHei',Arial,sans-serif;color:#111827;"">

                      <!-- Preheader（收件匣預覽文字；隱藏） -->
                      <div style=""display:none;font-size:1px;color:#f3f4f6;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;"">
                        您已成功報名「{safeEventName}」（內含入場 QR Code）
                      </div>

                      <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                        <tr>
                          <td align=""center"" style=""padding:24px"">

                            <!-- 內容容器 -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">

                              <!-- 頂部橫幅 -->
                              <tr>
                                <td style=""background:#0b0c1c;background-image:linear-gradient(135deg,#0b0c1c,#1a1b2c);padding:28px 28px 22px 28px;text-align:center;color:#ffffff;"">
                                  <div style=""font-size:14px;letter-spacing:2px;opacity:.85"">Apollo Astronomical Museum</div>
                                  <div style=""font-size:24px;font-weight:800;margin-top:4px"">報名成功 ✦ 登艙確認</div>
                                </td>
                              </tr>

                              <!-- 問候與開場 -->
                              <tr>
                                <td style=""padding:24px 28px 8px 28px"">
                                  <p style=""margin:0 0 12px 0;font-size:16px;"">{greeting}：</p>
                                  <p style=""margin:0 0 12px 0;font-size:16px;line-height:1.7"">
                                    歡迎加入我們的宇宙探險旅程！
                                  </p>
                                <p style=""margin:0 0 12px 0;font-size:16px;line-height:1.7"">
                                    很高興通知您，您已成功報名本次活動：
                                    <strong style=""color:#2B4C8C"">{safeEventName}</strong>。
                                  </p>
                                </td>
                              </tr>

                              <!-- 活動詳情卡片 -->
                              <tr>
                                <td style=""padding:0 28px 8px 28px"">
                                  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border:1px solid #e5e7eb;border-radius:10px;background:#f9fafb"">
                                    <tr>
                                      <td style=""padding:16px 16px 6px 16px;font-weight:700;color:#111827;"">活動詳情</td>
                                    </tr>
                                    <tr>
                                      <td style=""padding:0 16px 16px 16px"">
                                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""font-size:14px;color:#374151"">
                                          <tr>
                                            <td style=""padding:6px 0;width:86px;color:#6b7280"">活動時間</td>
                                            <td style=""padding:6px 0;"">{eventTime:yyyy-MM-dd HH:mm}</td>
                                          </tr>
                                          <tr>
                                            <td style=""padding:6px 0;color:#6b7280"">報到方式</td>
                                            <td style=""padding:6px 0;"">請於報到時出示下方 QR Code，以便完成入場手續。</td>
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>

                              <!-- QR Code 區塊 -->
                              <tr>
                                <td style=""padding:16px 28px 8px 28px;text-align:center"">
                                  <div style=""font-size:14px;color:#6b7280;margin-bottom:6px"">入場 QR Code</div>
                                  <img alt=""QR Code"" src=""cid:{cid}"" width=""220""
                                       style=""display:block;margin:0 auto;border:8px solid #f3f4f6;border-radius:12px;max-width:240px;height:auto;"" />
                                  <div style=""font-size:12px;color:#9ca3af;margin-top:8px"">若無法顯示圖片，請攜帶此信件於現場協助核對。</div>
                                </td>
                              </tr>

                              <!-- 退費提醒卡片 -->
                              <tr>
                                <td style=""padding:16px 28px 0 28px"">
                                  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%""
                                         style=""border:1px solid #e5e7eb;border-radius:10px;background:#fff7ed"">
                                    <tr>
                                      <td style=""padding:12px 16px;font-size:14px;line-height:1.7;color:#9a3412;"">
                                        <strong style=""display:inline-block;margin-right:.25rem;"">退費提醒：</strong>
                                        如需取消，請來電
                                        <a href=""tel:034533013 #202"" style=""color:#b45309;text-decoration:none;"">(03)453-3013 #202</a>
                                        或來信
                                        <a href=""mailto:apollotw2025@gmail.com"" style=""color:#b45309;text-decoration:none;"">apollotw2025@gmail.com</a>，
                                        我們將盡速為您辦理訂金退還。<br>
                                        <span style=""color:#7c2d12;font-size:12px;"">為加速處理作業，來電／來信請提供「報名編號」與姓名。</span>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>

                              <!-- 分隔線 -->
                              <tr>
                                <td style=""padding:12px 28px"">
                                  <hr style=""border:none;border-top:1px solid #e5e7eb;margin:0"">
                                </td>
                              </tr>

                              <!-- 收尾 -->
                              <tr>
                                <td style=""padding:16px 28px 24px 28px"">
                                  <p style=""margin:0 0 10px 0;font-size:15px;line-height:1.7"">
                                    期待與您相見，一同探索浩瀚宇宙的奧祕。
                                  </p>
                                    <p style=""margin:0 0 10px 0;font-size:15px;line-height:1.7"">
                                    若您臨時不克出席，也別急 —— 宇宙很大，下一班太空船永遠在路上。
                                  </p>
                                  <p style=""margin:0 0 10px 0;font-size:15px;line-height:1.7"">
                                    祝 順心如意，星辰大海！
                                  </p>
                                  <p style=""margin:0;color:#111827;font-weight:700"">阿波羅天文館</p>
                                </td>
                              </tr>

                                            </table>

                                            <!-- 頁尾 -->
                                            <div style=""font-size:12px;color:#9ca3af;margin-top:12px;"">此為系統信件，請勿直接回覆。</div>

                                          </td>
                                        </tr>
                                      </table>

                                    </body>
                                    </html>",
					// 純文字版本（含退費提醒）
					TextBody =
					$@"{greeting} 您好：

                    您已成功報名「{safeEventName}」。
                    活動時間：{eventTime:yyyy-MM-dd HH:mm}
                    報到方式：請於報到時出示本信件中的 QR Code 完成入場手續。

                    退費提醒：如需取消或改期，請來電 {servicePhone} 或來信 {serviceEmail}，我們將盡速為您辦理訂金退還。
                    為加速處理，來電／來信請提供「報名編號」與姓名。

                    期待與您相見！
                    阿波羅天文館"
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