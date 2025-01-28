namespace Banco_VivesBank.Utils.Jwt;

public interface IJwtService
{
    string GenerateToken(User.Models.User user);
}