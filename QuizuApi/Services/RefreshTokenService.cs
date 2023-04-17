using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;

namespace QuizuApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly QuizuApiDbContext _context;

        public RefreshTokenService(QuizuApiDbContext context)
        {
            _context = context;
        }

        public async Task<string> RetrieveOrGenerateRefreshTokenAsync(string userId)
        {
            var token = await RetrieveUserRefreshTokenAsync(userId);

            if (token is not null && token.IsActive && !token.IsRevoked)
            {
                return token.Value;
            }

            if (token is not null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
            return await GenerateRefreshTokenAsync(userId); ;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var dbToken = await RetrieveUserRefreshTokenAsync(userId);
            return !(dbToken is null || dbToken.IsRevoked || !dbToken.IsActive || refreshToken != dbToken.Value);
        }

        public async Task RevokeRefreshTokenAsync(string userId)
        {
            var dbToken = await RetrieveUserRefreshTokenAsync(userId);
            if (dbToken is null) return;
            dbToken.IsRevoked = true;
            dbToken.IsActive = false;
            _context.RefreshTokens.Update(dbToken);
            await _context.SaveChangesAsync();
        }

        private async Task<RefreshToken?> RetrieveUserRefreshTokenAsync(string userId)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId);
        }

        private async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            string refreshTokenValue = RandomStringGeneration(255);

            //TODO: encrypt token?

            var refreshToken = new RefreshToken()
            {
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsRevoked = false,
                UserId = userId,
                Value = refreshTokenValue
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshTokenValue;
        }

        private string RandomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyz_!@#$%^&*()";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
