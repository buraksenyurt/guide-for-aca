using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BalladMngr.Data
{
    /*
     * Çalışma zamanı DI servislerini Entity Framework DbContext türevimizi eklemek için kullanılan sınıf.
     * 
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddData(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<BalladMngrDbContext>(
                options => options.UseSqlite(configuration.GetConnectionString("BalladMngrDbConnection")) // SQLite veri tabanı bağlantı bilgisi konfigurasyon üstünden gelecek. Web API' nin appSettings.json dosyasından
                );

            // services.AddDbContext<BalladMngrDbContext>(
            //     options => options.UseSqlServer(configuration.GetConnectionString("BalladMngrDbConnection"))
            //     );

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<BalladMngrDbContext>());
            return services;
        }
    }
}