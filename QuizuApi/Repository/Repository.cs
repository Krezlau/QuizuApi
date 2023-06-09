using Microsoft.EntityFrameworkCore;
using QuizuApi.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;
using QuizuApi.Data;
using QuizuApi.Models.DTOs;
using QuizuApi.Models.Database;

namespace QuizuApi.Repository
{
    public class Repository<T> : IRepository<T> where T : AuditModel
    {
        protected readonly QuizuApiDbContext _context;
        internal DbSet<T> dbSet;

        public Repository(QuizuApiDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
        }

        public virtual async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            entity.IsDeleted = true;
            dbSet.Update(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            dbSet.Update(entity);
            await SaveAsync();
        }

        public async Task<PageResultDTO<T>> GetPageAsync(int pageNumber,
                                                    int pageSize,
                                                    Expression<Func<T, bool>>? filter = null,
                                                    string? includeProperties = null)
        {
            IQueryable<T> query = dbSet.Where(x => x.IsDeleted == false);

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
            var pageCount =  (int)Math.Ceiling((double)await query.CountAsync() / pageSize);

            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PageResultDTO<T>()
            {
                PageCount = pageCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                QueryResult = result
            };
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet.Where(x => x.IsDeleted == false);

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
            IQueryable<T> query = dbSet.Where(x => x.IsDeleted == false);

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
