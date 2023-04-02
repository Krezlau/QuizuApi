using Microsoft.EntityFrameworkCore;
using QuizuApi.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;
using QuizuApi.Data;

namespace QuizuApi.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly QuizuApiDbContext _context;
        internal DbSet<T> dbSet;

        public Repository(QuizuApiDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

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

        public async Task<T?> GetAsync(Expression<Func<T, bool>>? filter, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

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

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
