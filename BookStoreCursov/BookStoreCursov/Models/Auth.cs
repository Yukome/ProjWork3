namespace BookStoreCursov.Models
{
    public class Auth
    {
        public partial class LoginModel
        {
            public string Login { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public partial class RegisterModel
        {
            public string Login { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Surname { get; set; } = null!;
            public string? Patronymic { get; set; }
            public DateOnly? DateOfBirth { get; set; }
        }
    }
}
