using BalladMngr.Application.Dtos.User;
using BalladMngr.Domain.Entities;

namespace BalladMngr.Application.Common.Interfaces
{
    /*
     * Kullanıcı servisi response içeriği ile gelen bilgilere göre doğrulama işini üstlenen
     * ve id üstünden kullanıcı bilgisi döndüren fonksiyonları tarifleyen bir sözleşme sunuyor.
     */
    public interface IUserService
    {
        AuthenticationResponse Authenticate(AuthenticationRequest model);
        User GetById(int userId);
    }
}