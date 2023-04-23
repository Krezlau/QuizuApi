using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class RegisterRequestDTO
    {
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Username { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [MinLength(Constraints.PasswordLengthMin)]
        [MaxLength(Constraints.PasswordLengthMax)]
        public required string Password { get; set; }
        [MinLength(Constraints.PasswordLengthMin)]
        [MaxLength(Constraints.PasswordLengthMax)]
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
