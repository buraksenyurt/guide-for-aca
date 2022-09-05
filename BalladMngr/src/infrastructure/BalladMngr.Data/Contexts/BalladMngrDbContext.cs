using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalladMngr.Data.Contexts
{
    /*
     * DbContext nesnesi.
     * IApplicationDbContext türetmesine de dikkat edelim. Core.Application katmanındaki sözleşmeyi kullanıyoruz.
     * Bu DI servislerine Entity Context nesnesini eklerken işimize yarayacak.
     * 
     */
    public class BalladMngrDbContext
        : DbContext, IApplicationDbContext
    {
        public BalladMngrDbContext(DbContextOptions<BalladMngrDbContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
    }
}