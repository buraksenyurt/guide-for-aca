namespace BalladMngr.Domain.Settings
{
    /*
     * Shared projesine eklenecek EmailService, mail gönderme işini üstlenen bir servis.
     * Mail gönderimi sırasında kimden gittiği, Smtp ayarları gibi bilgileri appSettings.json içeriğinden alsak hiç fena olmaz.
     * Bu nedenle Domain altına appSettings altındaki MailSettings sekmesini işaret edecek bu sınıf eklendi.
     */
    public class MailSettings
    {
        public string From { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string DisplayName { get; set; }
    }
}