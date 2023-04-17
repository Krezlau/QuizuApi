using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class QuizUpdateRequestDTO
    {
        [MinLength(Constraints.TitleLengthMin)]
        [MaxLength(Constraints.TitleLengthMax)]
        public required string Title { get; set; }
        [MaxLength(Constraints.DescriptionLengthMax)]
        public string? Description { get; set; }
        [MaxLength(Constraints.AboutLengthMax)]
        public string? About { get; set; }
    }
}
