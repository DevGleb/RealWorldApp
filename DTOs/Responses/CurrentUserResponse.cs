namespace RealWorldApp.DTOs.Responses
{
    public class CurrentUserResponse
    {
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
