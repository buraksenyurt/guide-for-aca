using BalladMngr.Application.Songs.Queries.ExportSongs;
using System.Collections.Generic;

namespace BalladMngr.Application.Common.Interfaces
{
    /*
     * CSV dosya çıktısı üreten servisin sözleşmesi
     * IEmailService tarafında olduğu gibi bu hizmeti de DI mekanizmasına kolayca dahil edebileceğiz.
     */
    public interface ICsvBuilder
    {
        byte[] BuildFile(IEnumerable<SongRecord> songRecords);
    }
}