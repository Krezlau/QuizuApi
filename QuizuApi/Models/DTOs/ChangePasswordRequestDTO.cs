namespace QuizuApi.Models.DTOs
{
    public class ChangePasswordRequestDTO
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
