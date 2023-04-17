using QuizuApi.Models;

namespace QuizuApi.Services
{
    public interface IRefreshTokenService
    {
        Task<string> RetrieveOrGenerateRefreshTokenAsync(string userId);
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task RevokeRefreshTokenAsync(string userId);
    }
}
