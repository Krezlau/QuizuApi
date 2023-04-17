using QuizuApi.Models;

namespace QuizuApi.Services
{
    public interface IAccessTokenCreatorService
    {
        Task<string> GenerateJwtTokenAsync(string userId);
    }
}
