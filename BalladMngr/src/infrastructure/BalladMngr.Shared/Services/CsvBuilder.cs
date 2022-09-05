using CsvHelper;
using BalladMngr.Application.Songs.Queries.ExportSongs;
using BalladMngr.Application.Common.Interfaces;
using System.Globalization;

namespace BalladMngr.Shared.Services
{
    /*
     * Şarkı kaytılarını CSV dosyada geriye veren fonksiyonelliği içeren sınıfımız.
     * Application içerisinde tanımladığımız servis sözleşmesini uyguladığına dikkat edelim.
     * Yani servisin sözleşmesi Core katmanındaki Application projesinde, uyarlaması Infrastructure içerisindeki Shared projesinde.
     */
    public class CsvBuilder
        : ICsvBuilder
    {
        public byte[] BuildFile(IEnumerable<SongRecord> songRecords)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csvWriter.WriteRecords(songRecords);
                }
                return ms.ToArray();
            }
        }
    }
}