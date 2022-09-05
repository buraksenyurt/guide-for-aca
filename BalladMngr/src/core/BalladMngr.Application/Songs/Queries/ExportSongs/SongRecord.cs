using BalladMngr.Application.Common.Mappings;
using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;

namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * CSV içerisine hangi şarkı bilgilerini tutacağımızı belirten veri yapısı.
     * 
     * Bir nesne dönüşümü söz konusu olduğundan IMapFrom<T> uyarlaması var.
     * 
     */
    public class SongRecord
        :IMapFrom<Song>
    {
        public string Title { get; set; }
        public Status Status { get; set; }
    }
}