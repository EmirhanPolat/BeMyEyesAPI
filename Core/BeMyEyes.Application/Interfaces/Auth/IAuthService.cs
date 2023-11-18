using BeMyEyes.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeMyEyes.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<(int, string)> Registration(RegistrationModel model, string[] role);
        Task<(int, string)> Login(LoginModel model);
    }
}
