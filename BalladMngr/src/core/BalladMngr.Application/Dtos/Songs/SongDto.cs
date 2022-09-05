using AutoMapper;
using BalladMngr.Application.Common.Mappings;
using BalladMngr.Domain.Entities;

namespace BalladMngr.Application.Dtos.Songs
{
    /*
     * Bu aslında bir Data Transfer Object tipi.
     * 
     * Controller tarafından gelen istekleri ele alan MediatR nesneleri doğrudan Song tipi ile değil de,
     * Daha az özelliği içeren SongDto tipi ile çalışıyor. Son kullanıcıya da bu içerik verilecek.
     * 
     * Genelde Entity tipleri dolaşıma girdiğinde tüm özelliklerini sunmak istemediğimiz senaryolara dahil olabilir.
     * ViewModel'in bu durumlarda tüm entity nesnesi ile çalışmak yerine gerçekten ihtiyaç duyduğu özellikleri barındıran bir nesne ile çalışması doğru yaklaşımdır.
     * 
     * Bu DTO IMapFrom<T> arayüzünü kullanır. Bu kısım aslında AutoMapper'ın ilgilendiği arayüzdür.
     * IMapFrom içindeki Mapping metodu burada ele alınmış ve Language özelliği için ekstra bir işlem eklenmiştir.
     */
    public class SongDto
        : IMapFrom<Song>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public int Language { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Song, SongDto>()
                .ForMember(dest => dest.Language
                    , opt => opt.MapFrom(source => (int)source.Language)
                    );
        }
    }
}