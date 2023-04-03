using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QuizuApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizuApi.Services
{
    public class AccessTokenCreatorService : IAccessTokenCreatorService
    {
        private readonly UserManager<User> _userManager;
        private readonly byte[] secretKey;

        public AccessTokenCreatorService(UserManager<User> userManager, IConfiguration config)
        {
            _userManager = userManager;
            secretKey = Encoding.UTF8.GetBytes(config.GetSection("Appsettings:Key").Value);
        }

        public async Task<string> GenerateJwtTokenAsync(string userId)
        {
            var user = new User { Id = userId };
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
