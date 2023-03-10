using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Spice.Services
{
	public class EmailSender : IEmailSender
	{
		public EmailOptions Options { get; set; }
		public EmailSender(IOptions<EmailOptions> emailOptions)
		{
			Options = emailOptions.Value;
		}
		public Task SendEmailAsync(string email, string subject, string Message)
		{
			return Execute(Options.SendGridKey, email, subject, Message);
		}

		private Task Execute(string sendGridKey, string email, string subject, string message)
		{
			var client = new SendGridClient(sendGridKey);
			var msg = new SendGridMessage()
			{
				From = new EmailAddress("schhatrola779@rku.ac.in", "Spice Restaurant"),
				Subject = subject,
				PlainTextContent = message,
				HtmlContent = message
			};
			msg.AddTo(new EmailAddress(email));
			try
			{
				return client.SendEmailAsync(msg);
			}
			catch (Exception ex)
			{

			}
			return null;
		}
	}
}
