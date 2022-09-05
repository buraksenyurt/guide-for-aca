using System;

namespace BalladMngr.Application.Common.Exceptions
{
    public class SongNotFoundException
        : Exception
    {
        public SongNotFoundException(int songId)
          : base($"{songId} nolu kitap envanterde bulunamadı") { }
    }
}