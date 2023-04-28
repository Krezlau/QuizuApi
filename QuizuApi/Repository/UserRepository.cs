using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;
using QuizuApi.Repository.IRepository;

namespace QuizuApi.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(QuizuApiDbContext context) : base(context)
        {
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
    }
}
