using BalladMngr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Librarian.Application.Common.Interfaces
{
    /*
     * Entity Framework Core context nesnesine ait servis sözleşmemiz.
     * Onu da DI mekanizmasına kolayca dahil edip bağımlılığı çözümletebiliriz.
     * Yani infrastructe katmanındaki Data kütüphanesindeki BalladMngrDbContext'i bu arayüz üstünden sisteme entegre edebileceğiz.
     */
    public interface IApplicationDbContext
    {
        public DbSet<Song> Songs { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}