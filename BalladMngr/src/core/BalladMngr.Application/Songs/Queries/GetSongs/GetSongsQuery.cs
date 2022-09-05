using AutoMapper;
using AutoMapper.QueryableExtensions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Application.Dtos.Songs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Queries.GetSongs
{
    /*
     * Query ile tüm şarkı listesinin çekilmesi sürecini ele alıyoruz.
     * 
     * Talebe karşılık SongsViewModel nesnesi dönülüyor ki o da içinde SongDto tipinden bir liste barındırmakta.
     * 
     */
    public class GetSongsQuery
        : IRequest<SongsViewModel>
    {
    }

    public class GetSongsQueryHandler
        : IRequestHandler<GetSongsQuery, SongsViewModel>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetSongsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<SongsViewModel> Handle(GetSongsQuery request, CancellationToken cancellationToken)
        {
            SongsViewModel Songs;

            Songs = new SongsViewModel
            {
                SongList = await _context
                .Songs
                .ProjectTo<SongDto>(_mapper.ConfigurationProvider)
                .OrderBy(b => b.Title)
                .ToListAsync(cancellationToken)
            }; // normal EF üstünden repository'ye gidip veriyi çekiyor

            return Songs;
        }
    }
}