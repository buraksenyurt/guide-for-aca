namespace BalladMngr.Domain.Entities
{
    // İleride authentication gibi noktalarda kullanacağımız kullanıcı içindir.
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}