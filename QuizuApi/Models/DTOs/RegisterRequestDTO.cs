using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class RegisterRequestDTO
    {
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string RepeatPassword { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Location { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Name { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Surname { get; set; }
    }
}
