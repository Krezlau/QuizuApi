using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;

namespace QuizuApi.Repository.IRepository
{
    public interface IPlayRepository : IRepository<QuizPlay>
    {
        Task SavePlayAsync(string userId, Guid quizId, UserPlayResultDTO answers);
        Task<double> GetPercentageOfUsersYouBeatAsync(Guid quizId, int score);
        Task<(double avgScore, double avgTimeTaken)> GetAverageScoreForQuizAsync(Guid quizId);
        Task<(double correctPercentage, double avgTimeTaken)> GetAverageScoreForQuestionAsync(Guid answerId);
    }
}
