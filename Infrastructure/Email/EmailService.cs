using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;

namespace QLSK.Infrastructure.Email;

public class EmailService(IOptions<EmailOptions> options)
{
    private readonly EmailOptions cfg = options.Value;

    public virtual async Task Gui(
        string to,
        string subject,
        string body,
        byte[]? attachment = null)
    {
        if (string.IsNullOrWhiteSpace(cfg.Host))
        {
            throw new ApiException(
                503,
                "EMAIL_NOT_CONFIGURED",
                "Chưa cấu hình SMTP."
            );
        }

        using var mail = new MailMessage(
            cfg.From,
            to,
            subject,
            body
        );

        if (attachment != null)
        {
            var stream = new MemoryStream(attachment);

            mail.Attachments.Add(
                new Attachment(
                    stream,
                    "ve.pdf",
                    "application/pdf"
                )
            );
        }

        using var smtp = new SmtpClient(cfg.Host, cfg.Port)
        {
            EnableSsl = cfg.EnableSsl,
            Credentials = new NetworkCredential(
                cfg.User,
                cfg.Password
            )
        };

        await smtp.SendMailAsync(mail);
    }
}