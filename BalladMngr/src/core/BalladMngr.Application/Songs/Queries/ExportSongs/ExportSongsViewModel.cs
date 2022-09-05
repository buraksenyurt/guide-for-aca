namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * Şarkıların bir çıktısını CSV olarak verdiğimizde kullanılan ViewModel nesnemiz.
     * Bunu ExportSongsQuery ve Handler tipi kullanmakta.
     * Dosyanın adını, içeriğin tipini ve byte[] cinsinden içeriği tutuyor.
     * Belki byte[] yerine 64 bit encode edilmiş string içerik de verebiliriz. 
     */
    public class ExportSongsViewModel
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}