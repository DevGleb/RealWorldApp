namespace RealWorldApp.Models
{
    public class LoginRequest
    {
        public LoginUserDto User { get; set; } = new();
    }

    public class LoginUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
