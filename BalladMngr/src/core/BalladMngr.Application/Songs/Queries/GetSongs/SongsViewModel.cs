using BalladMngr.Application.Dtos.Songs;
using System.Collections.Generic;

namespace BalladMngr.Application.Songs.Queries.GetSongs
{
    /*
     * Şarkı listesinin tamamını çeken Query'nin çalıştığı ViewModel nesnesi
     * 
     */
    public class SongsViewModel
    {
        public IList<SongDto> SongList { get; set; }
    }
}