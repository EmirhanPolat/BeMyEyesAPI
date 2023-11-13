using UserManagement.Data.Models;

namespace BeMyEyesFirst.Services
{
    public interface IAuthService
    {
        Task<(int, string)> Registration(RegistrationModel model, string[] role);
        Task<(int, string)> Login(LoginModel model);
    }
}
