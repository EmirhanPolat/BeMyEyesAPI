using BeMyEyes.Application.Interfaces.Auth;
using BeMyEyes.Application.Tokens;
using BeMyEyes.Domain.Entities;
using BeMyEyes.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeMyEyes.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ITokenService _tokenService;
        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<(int, string)> Registration(RegistrationModel model, string[] roles)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);

            if (userExists != null)
                return (0, "User already exists");

            User user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FullName = model.FullName,
            };
            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
                return (0, "User creation failed! Please check user details and try again.");

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

                if (await roleManager.RoleExistsAsync(role))
                    await userManager.AddToRoleAsync(user, role);
            }

            return (1, "User created successfully!");
        }

        public async Task<(int, string)> Login(LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);

            if (user == null)
                return (0, "Invalid username");
            if (!await userManager.CheckPasswordAsync(user, model.Password))
                return (0, "Invalid password");

            var userRoles = await userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, user.UserName),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = _tokenService.CreateToken(user, userRoles).Result.ToString();
            return (1, token);
        }
    }
}
