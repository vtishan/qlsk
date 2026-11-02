using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;

namespace QLSK.Infrastructure.Email;

public class EmailService(IOptions<EmailOptions> tuyChonEmail)
{
    private readonly EmailOptions cauHinh = tuyChonEmail.Value;

    public virtual async Task GuiEmailVe(
        string emailNhan,
        string tieuDe,
        string noiDung,
        byte[]? tepPdfVe = null)
    {
        if (string.IsNullOrWhiteSpace(cauHinh.MayChuSmtp))
        {
            throw new ApiException(
                503,
                "EMAIL_NOT_CONFIGURED",
                "Chưa cấu hình SMTP."
            );
        }

        using var thuEmail = new MailMessage(
            cauHinh.EmailGui,
            emailNhan,
            tieuDe,
            noiDung
        );

        if (tepPdfVe != null)
        {
            var luongPdf = new MemoryStream(tepPdfVe);

            thuEmail.Attachments.Add(
                new Attachment(
                    luongPdf,
                    "ve.pdf",
                    "application/pdf"
                )
            );
        }

        using var clientGuiEmailSmtp = new SmtpClient(cauHinh.MayChuSmtp, cauHinh.CongSmtp)
        {
            EnableSsl = cauHinh.BatMaHoaSsl,
            Credentials = new NetworkCredential(
                cauHinh.TenDangNhapSmtp,
                cauHinh.MatKhauSmtp
            )
        };

        await clientGuiEmailSmtp.SendMailAsync(thuEmail);
    }
}