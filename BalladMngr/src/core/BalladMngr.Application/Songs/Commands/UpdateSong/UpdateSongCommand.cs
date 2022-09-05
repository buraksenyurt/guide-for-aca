using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.UpdateSong
{
    /*
     * Bir şarkının bilgilerini güncellemek isteyebilirim. Adı, içeriği, durumu vesaire.
     * Topluca bunların güncellemesini ele alan Command'ın beklediği özellikler aşağıdaki gibidir.
     * 
     * Handler sınıfı da bu Command'i kullanarak repository üzerinde gerekli güncellemeleri yapar. 
     * Id ile gelen kitap bilgisi bulunamazsa ortama SongNotFoundException isimli bizim yazdığımız bir istisna tipi fırlatılır.
     * 
     */
    public partial class UpdateSongCommand : IRequest
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public Language Language { get; set; }
        public Status Status { get; set; }
    }

    public class UpdateSongCommandHandler : IRequestHandler<UpdateSongCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateSongCommand request, CancellationToken cancellationToken)
        {
            var s = await _context.Songs.FindAsync(request.SongId);
            if (s == null)
            {
                throw new SongNotFoundException(request.SongId);
            }
            s.Title = request.Title;
            s.Lyrics = request.Lyrics;
            s.Language = request.Language;
            s.Status = request.Status;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}