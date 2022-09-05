using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.DeleteSong
{
    /*
     * beste silme işini ele aldığımı Command ve Handler tipleri.
     * 
     * Bir besteyi silmek için talebin içinde Id bilgisinin olması yeterli. Command buna göre düzenlendi.
     * Silme operasyonunu ele alan Handler tipimiz yine ilgili kitabı envanterde arıyor.
     * Eğer bulamazsa SongNotFoundException fırlatılıyor.
     * 
     */
    public class DeleteSongCommand
        : IRequest
    {
        public int SongId { get; set; }
    }

    public class DeleteSongCommandHandler
        : IRequestHandler<DeleteSongCommand>
    {
        private readonly IApplicationDbContext _context;
        public DeleteSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteSongCommand request, CancellationToken cancellationToken)
        {
            var b = await _context.Songs
              .Where(l => l.Id == request.SongId)
              .SingleOrDefaultAsync(cancellationToken);

            if (b == null)
            {
                throw new SongNotFoundException(request.SongId);
            }

            _context.Songs.Remove(b);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}