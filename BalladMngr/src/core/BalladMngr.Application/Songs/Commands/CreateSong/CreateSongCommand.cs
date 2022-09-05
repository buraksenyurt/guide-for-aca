using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.CreateSong
{
    /*
     * Şarkı listesine yeni bir beste ekleme işini üstlenen MediatR tipleri.
     * Komut şarkı için gerekli parametreleri alırken geriye insert sonrası oluşan bir int değer(kuvvetle muhtemel primary key id değeri) dönüyor.
     * Handler sınıfı IApplicationDbContext üstünden gelen Entity Context nesnesini kullanarak besteyi repository'ye ekliyor.
     * 
     */
    public class CreateSongCommand
        : IRequest<int>
    {
        public string Title { get; set; }
        public Language Language{ get; set; }
        public string Lyrics { get; set; }
    }

    public class CreateSongCommandHandler
        : IRequestHandler<CreateSongCommand, int>
    {
        private readonly IApplicationDbContext _context;
        public CreateSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CreateSongCommand request, CancellationToken cancellationToken)
        {
            var s = new Song
            {
                Title = request.Title,
                Lyrics=request.Lyrics,
                Language=request.Language,
                Status=Status.Draft
            };
            _context.Songs.Add(s);
            await _context.SaveChangesAsync(cancellationToken);

            return s.Id;
        }
    }
}