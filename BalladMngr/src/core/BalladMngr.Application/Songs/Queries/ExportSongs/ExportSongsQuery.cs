using AutoMapper;
using AutoMapper.QueryableExtensions;
using Librarian.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using BalladMngr.Application.Common.Interfaces;

namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * 
     * Query nesnemiz.
     * Talebe karşılık bir ViewModel döndürüleceğini tanımlıyor.
     * 
     */
    public class ExportSongsQuery
        : IRequest<ExportSongsViewModel>
    {
    }

    /*
    * Handler tipimiz.
    * Gelen sorguya ele alıp uygun ViewModel nesnesinin döndürülmesi sağlanıyor.
    * 
    * Kullanması için gereken bağımlılıkları Constructor Injection ile içeriye alıyoruz.
    * Buna göre CSV üretici, AutoMapper nesne dönüştürücü ve EF Core DbContext servislerini içeriye alıyoruz.
    * 
    * Şarkı listesini çektiğimiz LINQ sorgusunda ProjectTo metodunu nasıl kullandığımız dikkat edelim.
    * Listenin SongRecord tipinden nesnelere dönüştürülmesi noktasında AutoMapper'ın çalışma zamanı sorumlusu kimse o kullanılıyor olacak.
    * Nitekim SongRecord tipinin IMapFrom<Song> tipini uyguladığını düşünecek olursak çalışma zamanı Song üstünden gelen özelliklerden eş düşenleri, SongRecord karşılıklarına alacak.
    */
    public class ExportSongsQueryHandler : IRequestHandler<ExportSongsQuery, ExportSongsViewModel>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICsvBuilder _csvBuilder;
        public ExportSongsQueryHandler(IApplicationDbContext context, IMapper mapper, ICsvBuilder csvBuilder)
        {
            _context = context;
            _mapper = mapper;
            _csvBuilder = csvBuilder;
        }
        public async Task<ExportSongsViewModel> Handle(ExportSongsQuery request, CancellationToken cancellationToken)
        {
            var viewModel = new ExportSongsViewModel();

            var list = await _context.Songs
                .ProjectTo<SongRecord>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewModel.FileName = "Songs.csv";
            viewModel.ContentType = "text/csv";
            viewModel.Content = _csvBuilder.BuildFile(list);

            return await Task.FromResult(viewModel);
        }
    }
}