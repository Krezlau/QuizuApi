using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using System.Net;

namespace QuizuApi.Repository
{
    public class QuestionAnswerRepository : IQuestionAnswerRepository
    {
        private readonly QuizuApiDbContext _context;

        public QuestionAnswerRepository(QuizuApiDbContext context)
        {
            _context = context;
        }

        public async Task<Question> CreateAsync(QuestionRequestDTO question)
        {
            bool outcome = Guid.TryParseExact(question.QuizId, "D", out Guid quizGuid);

            if (!outcome)
            {
                throw new Exception("Invalid quiz id.");
            }

            Question q = new Question()
            {
                Content = question.Content,
                QuizId = quizGuid,
            };

            List<Answer> answers = question.Answers.Select(a => new Answer()
            {
                Content = a.Content,
                IsCorrect = a.IsCorrect,
                QuestionId = q.Id
            }).ToList();

            q.Answers = answers;

            await _context.Questions.AddAsync(q);
            await SaveAsync();
            return q;
        }

        public async Task DeleteAsync(Question question)
        {
            _context.Answers.RemoveRange(question.Answers);
            _context.Questions.Remove(question);
            await SaveAsync();
        }

        public async Task<List<Question>> GetAllAsync(Guid quizId)
        {
            return await _context.Questions.Where(q => q.QuizId == quizId).Include("Answers").ToListAsync();
        }

        public async Task<Question?> GetAsync(Guid questionId)
        {
            return await _context.Questions.Where(q => q.Id == questionId).Include("Answers").FirstOrDefaultAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Question question)
        {
            _context.Answers.UpdateRange(question.Answers);
            _context.Questions.Update(question);
            await SaveAsync();
        }

        public async Task CreateAnswerAsync(AnswerRequestDTO answer, Guid questionId)
        {
            _context.Answers.Add(new Answer() { Content = answer.Content, IsCorrect = answer.IsCorrect, QuestionId = questionId});
            await SaveAsync();
        }

        public async Task DeleteAnswerAsync(Answer answer)
        {
            _context.Answers.Remove(answer);
            await SaveAsync();
        }
    }
}
