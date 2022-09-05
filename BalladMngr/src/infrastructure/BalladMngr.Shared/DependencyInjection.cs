using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Settings;
using BalladMngr.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BalladMngr.Shared
{
    /*
     * 
     * Çağıran ortamdaki DI servislerine Shared tarafından gelen bağımlılıkları yüklememizi kolaylaştıran sınıf.
     * BalladMngr.Application'daki DependencyInjection sınıfı ile aynı teoriyi kullanıyor ve yine
     * kuvvetle muhtemel buradaki servisleri tüketecek olan Web API projesindeki Startup sınıfındaki ConfigureServices içinden çağırılacak
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICsvBuilder, CsvBuilder>();

            return services;
        }
    }
}