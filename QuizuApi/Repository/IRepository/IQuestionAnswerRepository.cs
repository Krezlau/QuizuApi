using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;

namespace QuizuApi.Repository.IRepository
{
    public interface IQuestionAnswerRepository
    {
        Task<List<Question>> GetAllAsync(Guid quizId);
        Task<Question?> GetAsync(Guid questionId);
        Task<Question> CreateAsync(QuestionRequestDTO question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(Question question);
        Task SaveAsync();
        Task DeleteAnswerAsync(Answer answer);
        Task CreateAnswerAsync(AnswerRequestDTO answer, Guid questionId);
    }
}
