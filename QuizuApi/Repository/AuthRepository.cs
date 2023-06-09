using Microsoft.AspNetCore.Identity;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using System.Text;
using QuizuApi.Data;
using Microsoft.EntityFrameworkCore;
using QuizuApi.Services;
using QuizuApi.Exceptions;
using QuizuApi.Models.Database;

namespace QuizuApi.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly QuizuApiDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccessTokenCreatorService _accessTokenService;
        private readonly IAccessTokenReaderService _tokenReaderService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthRepository(QuizuApiDbContext context,
                              UserManager<User> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IAccessTokenCreatorService tokenService,
                              IRefreshTokenService refreshTokenService,
                              IAccessTokenReaderService tokenReaderService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _accessTokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _tokenReaderService = tokenReaderService;
        }

        public async Task<bool> IsUniqueUserAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            return user is null;
        }

        public async Task<(string accessToken, string userId, string refreshToken, string username)> LoginUserAsync(LoginRequestDTO loginRequestDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDTO.Email);

            if (user is null)
            {
                throw new AuthException("Incorrect email.");
            }

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (!isValid)
            {
                throw new AuthException("Incorrect password.");
            }

            string token = await _accessTokenService.GenerateJwtTokenAsync(user.Id);
            string refreshToken = await _refreshTokenService.RetrieveOrGenerateRefreshTokenAsync(user.Id);

            return (token, user.Id, refreshToken, user.UserName);
        }

        public async Task<bool> RegisterUserAsync(RegisterRequestDTO registerRequestDTO)
        {
            var user = new User()
            {
                UserName = registerRequestDTO.Username,
                Email = registerRequestDTO.Email,
                JoinedAt = DateTime.Now,
                Location = registerRequestDTO.Location,
                Name = registerRequestDTO.Name,
                Surname = registerRequestDTO.Surname,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(user, registerRequestDTO.Password);

            if (result.Succeeded)
            {
                if (!(await _roleManager.RoleExistsAsync("user")))
                {
                    await _roleManager.CreateAsync(new IdentityRole("user"));
                }

                await _userManager.AddToRoleAsync(user, "user");
                return true;
            }
            else
            {
                StringBuilder errors = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    errors.AppendLine(error.Description);
                }
                throw new AuthException(errors.ToString().TrimEnd());
            }
        }

        public async Task<string> RefreshAsync(string accessToken, string refreshToken)
        {
            string userId;
            try
            {
                userId = _tokenReaderService.ReadUserId(accessToken);
            }
            catch (Exception ex)
            {
                throw new AuthException("Invalid access token.", ex);
            }


            if (!await _refreshTokenService.ValidateRefreshTokenAsync(userId, refreshToken))
            {
                throw new AuthException("Ivalid refresh token.");
            }

            return await _accessTokenService.GenerateJwtTokenAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(userId);
                return true;
            }
            else
            {
                var errors = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    errors.AppendLine(error.Description);
                }
                throw new AuthException(errors.ToString().TrimEnd());
            }
        }
    }
}
