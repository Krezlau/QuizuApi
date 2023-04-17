using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;

namespace QuizuApi.Repository.IRepository
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<QuizActivityDTO> FetchActivityInfoAsync(Guid quizId, string? userId = null);
        Task<bool> CheckIfTitleAvailable(string title);
    }
}
