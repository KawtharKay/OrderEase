using static Application.Commands.LoginUser;

namespace Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(LoginResponse response);
    }
}
