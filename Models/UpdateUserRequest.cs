namespace RealWorldApp.Models
{
    public class UpdateUserRequest
    {
        public UpdateUserDto User { get; set; } = new();
    }

    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
    }
}
