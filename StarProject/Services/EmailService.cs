using NETCore.MailKit.Core;
using System.Threading.Tasks;

public class EmailService
{
	private readonly IEmailService _emailService;

	public EmailService(IEmailService emailService)
	{
		_emailService = emailService;
	}

	public async Task SendEmailAsync()
	{
		await _emailService.SendAsync(
			"收件人_email@gmail.com",
			"測試信件主旨",
			"這是一封測試信件內容",
			true // 是否為 HTML
		);
	}
}
