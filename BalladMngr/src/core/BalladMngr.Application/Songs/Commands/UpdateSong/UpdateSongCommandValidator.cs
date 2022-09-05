using FluentValidation;
using BalladMngr.Application.Common.Interfaces;

namespace BalladMngr.Application.Songs.Commands.UpdateSong
{
    /*
     * Şarkı bilgilerinin güncellendiği Command nesnesi için hazırlanmış doğrulama tipi. 
     * Esasında CreateSongCommandValidator ile neredeyse aynı.
     */
    public class UpdateSongCommandValidator : AbstractValidator<UpdateSongCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSongCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(v => v.Title)
              .NotEmpty().WithMessage("Şarkının başlığı olmalı!")
              .MaximumLength(100).WithMessage("Bu şarkının adı çok uzun!");

            RuleFor(v => v.Lyrics)
              .NotEmpty().WithMessage("Birkaç mısra da olsa sözler olmalı!")
              .MinimumLength(50).WithMessage("Bence en az 50 karakterden oluşan bir metin olmalı!")
              .MaximumLength(1000).WithMessage("Ne çok söz varmış. Şarkıyı kısaltalım.");
        }
    }
}