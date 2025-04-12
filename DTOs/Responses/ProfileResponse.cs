using RealWorldApp.DTOs.Responses;

namespace RealWorldApp.DTOs.Responses
{
    public class ProfileResponse
    {
        public AuthorResponse Profile { get; set; } = new();
    }
}
