using QuizuApi.Models.DTOs;
using System.Linq.Expressions;

namespace QuizuApi.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<PageResultDTO<T>> GetPageAsync(int pageNumber,
                                       int pageSize,
                                       Expression<Func<T, bool>>? filter = null,
                                       string? includeProperties = null);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null,
                          bool tracked = true,
                          string? includeProperties = null);
        Task CreateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAsync();
    }
}
