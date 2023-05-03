using QuizuApi.Models.Database;

namespace QuizuApi.Repository.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<int> FetchUserQuizCountAsync(string userId);
        Task<int> FetchUserFollowersCount(string userId);
        Task<bool> CheckIfUsernameAvailable(string username);
    }
}
