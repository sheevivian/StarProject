using System;

namespace StarProject.Models
{
	public class MailMessageModel
	{
		public string To { get; set; }          // 收件人
		public string Subject { get; set; }     // 郵件標題
		public string Body { get; set; }        // 郵件內容
	}
}
