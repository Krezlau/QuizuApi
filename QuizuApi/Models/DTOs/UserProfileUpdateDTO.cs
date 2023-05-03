using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class UserProfileUpdateDTO
    {
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Username { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public string? Name { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public string? Surname { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public string? Location { get; set; }
        [MaxLength(Constraints.AboutLengthMax)]
        public string? About { get; set; }
    }
}
