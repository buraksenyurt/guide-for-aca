using BalladMngr.Domain.Enums;

namespace BalladMngr.Domain.Entities
{
     // Beste ile ilgili temel bilgileri içerir.
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public Status Status { get; set; }
        public Language Language { get; set; }
    }
}