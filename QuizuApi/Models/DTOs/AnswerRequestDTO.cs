using QuizuApi.Models.Database;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class AnswerRequestDTO
    {
        public AnswerRequestDTO()
        {

        }

        [SetsRequiredMembers]
        public AnswerRequestDTO(Answer answer)
        {
            Content = answer.Content;
            IsCorrect = answer.IsCorrect;
        }

        [MinLength(Constraints.AnswerLengthMin)]
        [MaxLength(Constraints.AnswerLengthMax)]
        public required string Content { get; set; }
        public required bool IsCorrect { get; set; }
    }
}
