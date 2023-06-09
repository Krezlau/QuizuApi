using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using System.Linq;
using System.Linq.Expressions;

namespace QuizuApi.Repository
{
    public class UserRepository : IRepository<User>, IUserRepository
    {
        private readonly IQuizRepository _quizRepository;

        protected readonly QuizuApiDbContext _context;
        internal DbSet<User> dbSet;

        public UserRepository(QuizuApiDbContext context, IQuizRepository quizRepository)
        {
            _context = context;
            this.dbSet = _context.Set<User>();
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

        public async Task DeleteAsync(User entity)
        {
            //_context.UserFollows.RemoveRange(await _context.UserFollows.Where(x => x.UserFollowedId == entity.Id).ToArrayAsync());
            //_context.UserFollows.RemoveRange(await _context.UserFollows.Where(x => x.UserFollowingId == entity.Id).ToArrayAsync());
            var quizzes = await _context.Quizzes.Where(q => q.AuthorId == entity.Id).ToArrayAsync();
            foreach (var quiz in quizzes)
            {
                await _quizRepository.DeleteAsync(quiz);
            }

            //var plays = await _context.QuizPlays.Where(qp => qp.UserId == entity.Id).ToArrayAsync();
            //foreach (var qp in plays)
            //{
            //    // TODO
            //    // delete via quizplays 
            //}

            var likes = await _context.QuizLikes.Where(x => x.UserId == entity.Id).ToArrayAsync();
            foreach (var like in likes)
            {
                like.IsDeleted = true;
            }
            _context.QuizLikes.UpdateRange(likes);

            var comments = await _context.QuizComments.Where(x => x.AuthorId == entity.Id).ToArrayAsync();
            foreach (var comment in comments)
            {
                comment.IsDeleted = true;
            }
            _context.QuizComments.UpdateRange(comments);

            entity.IsDeleted = true;
            dbSet.Update(entity);

            await _context.SaveChangesAsync();
        }

        public async Task<PageResultDTO<User>> GetPageAsync(int pageNumber, int pageSize, Expression<Func<User, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<User> query = dbSet.Where(x => x.IsDeleted == false);

            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (includeProperties is not null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            var pageCount = (int)Math.Ceiling((double)await query.CountAsync() / pageSize);

            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PageResultDTO<User>()
            {
                PageCount = pageCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                QueryResult = result
            };
        }

        public async Task<List<User>> GetAllAsync(Expression<Func<User, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<User> query = dbSet.Where(x => x.IsDeleted == false);

            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (includeProperties is not null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>>? filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<User> query = dbSet.Where(x => x.IsDeleted == false);

            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (includeProperties is not null && includeProperties != "")
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            dbSet.Update(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
