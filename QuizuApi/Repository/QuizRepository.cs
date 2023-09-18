using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;

namespace QuizuApi.Repository
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(QuizuApiDbContext context) : base(context)
        {
        }

        public async Task<QuizActivityDTO> FetchActivityInfoAsync(Guid quizId, string? userId = null)
        {
            var result = new QuizActivityDTO()
            {
                LikesCount = await _context.QuizLikes.CountAsync(l => l.QuizId == quizId && l.IsDeleted == false),
                CommentsCount = await _context.QuizComments.CountAsync(c => c.QuizId == quizId && c.IsDeleted == false),
                PlaysCount = await _context.QuizPlays.CountAsync(p => p.QuizId == quizId && p.IsDeleted == false)
            };

            if (userId is not null)
            {
                result.IsLikedByUser = await _context.QuizLikes.AnyAsync(l => l.QuizId == quizId && l.UserId == userId);
                result.IsAlreadyPlayedByUser = await _context.QuizPlays.AnyAsync(p => p.QuizId == quizId && p.UserId == userId);
            }
            return result;
        }

        public async Task<bool> CheckIfTitleAvailable(string title)
        {
            return !await dbSet.AnyAsync(q => q.Title == title);
        }

        public override async Task CreateAsync(Quiz entity)
        {
            await base.CreateAsync(entity);
            _context.QuizSettings.Add(new QuizSettings()
            {
                AllowReplays = true,
                AnswerTimeS = 10,
                QuestionsPerPlay = -1, // all questions
                QuizId = entity.Id,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Quiz entity)
        {
            var questions = await _context.Questions.Where(q => q.QuizId == entity.Id).ToArrayAsync();
            foreach (var q in questions)
            {
                var answers = await _context.Answers.Where(a => a.QuestionId == q.Id).ToArrayAsync();
                foreach (var a in answers)
                {
                    a.IsDeleted = true;
                }
                _context.Answers.UpdateRange(answers);
                q.IsDeleted = true;
            }
            _context.Questions.UpdateRange(questions);

            //_context.QuizLikes.RemoveRange(await _context.QuizLikes.Where(ql => ql.QuizId == entity.Id).ToArrayAsync());
            //_context.QuizComments.RemoveRange(await _context.QuizComments.Where(qc => qc.QuizId == entity.Id).ToArrayAsync());
            //_context.QuizPlays.RemoveRange(await _context.QuizPlays.Where(qp => qp.QuizId == entity.Id).ToArrayAsync());
            await base.DeleteAsync(entity);
        }

        public async Task<PageResultDTO<Quiz>> FuzzySearchQuizzes(string query, int pageNumber, int pageSize)
        {
            var fuzzyScores = Process.ExtractSorted(query, await _context.Quizzes.Select(q => q.Title).ToArrayAsync());

            // Filter the jobs that have a fuzzy score above a certain threshold
            var threshold = 50;
            var filteredResults = fuzzyScores.Where(x => x.Score >= threshold).Select(x => x.Value);

            var searchResults = filteredResults.Skip(pageNumber*pageSize).Take(pageSize);
            var pageCount = (int)Math.Ceiling((double)filteredResults.Count() / pageSize);

            var quizResults = searchResults.Select(q => _context.Quizzes.Where(qz => qz.Title == q).Include(q => q.Author).Include(q => q.Tags).First()).ToList();

            return new PageResultDTO<Quiz>()
            {
                PageCount = pageCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                QueryResult = quizResults
            };
        }
    }
}
