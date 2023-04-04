using QuizuApi.Models.DTOs;

namespace QuizuApi.Repository.IRepository
{
    public interface IAuthRepository
    {
        Task<bool> IsUniqueUserAsync(string username);
        Task<(string accessToken, string userId, string refreshToken)> LoginUserAsync(LoginRequestDTO loginRequestDTO);
        Task<bool> RegisterUserAsync(RegisterRequestDTO registerRequestDTO);
        Task<string> RefreshAsync(string accessToken, string refreshToken);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
