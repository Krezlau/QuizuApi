using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;
using QuizuApi.Repository.IRepository;

namespace QuizuApi.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IQuizRepository _quizRepository;

        public UserRepository(QuizuApiDbContext context, IQuizRepository quizRepository) : base(context)
        {
            _quizRepository = quizRepository;
        }

        public async Task<bool> CheckIfUsernameAvailable(string username)
        {
            return !await _context.Users.AnyAsync(u => u.UserName == username);
        }

        public async Task<int> FetchUserFollowersCount(string userId)
        {
            return await _context.UserFollows.CountAsync(uf => uf.UserFollowedId == userId);
        }

        public async Task<int> FetchUserQuizCountAsync(string userId)
        {
            return await _context.Quizzes.CountAsync(q => q.AuthorId == userId);
        }

        public override async Task DeleteAsync(User entity)
        {
            _context.UserFollows.RemoveRange(await _context.UserFollows.Where(x => x.UserFollowedId == entity.Id).ToArrayAsync());
            _context.UserFollows.RemoveRange(await _context.UserFollows.Where(x => x.UserFollowingId == entity.Id).ToArrayAsync());
            var quizzes = await _context.Quizzes.Where(q => q.AuthorId == entity.Id).ToArrayAsync();
            foreach (var quiz in quizzes)
            {
                await _quizRepository.DeleteAsync(quiz);
            }

            var plays = await _context.QuizPlays.Where(qp => qp.UserId == entity.Id).ToArrayAsync();
            foreach (var qp in plays)
            {
                // TODO
                // delete via quizplays 
            }

            _context.QuizLikes.RemoveRange(await _context.QuizLikes.Where(x => x.UserId == entity.Id).ToArrayAsync());
            _context.QuizComments.RemoveRange(await _context.QuizComments.Where(x => x.AuthorId == entity.Id).ToArrayAsync());

            await _context.SaveChangesAsync();
            
            await base.DeleteAsync(entity);
        }
    }
}
