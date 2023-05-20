using QuizuApi.Models.Database;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class AnswerDTO
    {
        [SetsRequiredMembers]
        public AnswerDTO(Answer answer)
        {
            Id = answer.Id;
            Content = answer.Content;
            IsCorrect = answer.IsCorrect;
        }

        public required Guid Id { get; set; }
        public required string Content { get; set; }
        public required bool IsCorrect { get; set; }
    }
}
