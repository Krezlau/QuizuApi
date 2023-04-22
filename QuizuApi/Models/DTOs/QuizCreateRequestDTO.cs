using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class QuizCreateRequestDTO
    {
        [MinLength(Constraints.TitleLengthMin)]
        [MaxLength(Constraints.TitleLengthMax)]
        public required string Title { get; set; }
    }
}
