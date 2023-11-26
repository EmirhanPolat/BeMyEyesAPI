using BeMyEyes.Domain.Models;

namespace BeMyEyes.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<(int, string)> Registration(RegistrationModel model, string[] role);
        Task<(int, string)> Login(LoginModel model);
    }
}
