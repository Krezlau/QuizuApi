namespace QuizuApi.Models.DTOs
{
    public class UserProfileUpdateDTO
    {
        public required string Username { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Location { get; set; }
        public string? About { get; set; }
    }
}
