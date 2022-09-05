using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Application.Dtos.Email;
using BalladMngr.Domain.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BalladMngr.Shared.Services
{
    /*
     * Mail gönderme işini yapan asıl sınıf.
     * Tahmin edileceği üzere tüm servis sözleşmelerinin olduğu Core katmanındaki Application projesindeki arayüzü kullanıyor.
     * Loglama ve Email sunucu ayarlarını almak için gerekli bağımlılıklar Constructor üzerinden enjekte edilmekte.
     * SendAsync metodu EmailDto isimli Data Transfer Object üstünden gelen bilgilerdeki kişiye mail olarak gönderiyor.
     * Mail gönderimi sırasında bir istisna olması oldukça olası. Sonuçta bir SMTP sunucum bile yok. 
     * Bunu hata loglarında veya exception tablosunda ayrıca anlayabilmek için SendMailException isimli bir istisna tipi kullanılıyor.
     */
    public class EmailService
        : IEmailService
    {
        private ILogger<EmailService> _logger { get; }
        private MailSettings _mailSettings { get; }

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailDto request)
        {
            try
            {
                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(request.From ?? _mailSettings.From)
                };
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var builder = new BodyBuilder { HtmlBody = request.Body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw new SendEmailException(ex.Message);
            }
        }
    }
}