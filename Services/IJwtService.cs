namespace RealWorldApp.Services
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email);
    }
}
