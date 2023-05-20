using QuizuApi.Models.Database;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class QuestionDTO
    {
        [SetsRequiredMembers]
        public QuestionDTO(Question question)
        {
            Id = question.Id;
            Content = question.Content;
            Answers = question.Answers.Select(a => new AnswerDTO(a)).ToList();
        }

        public required Guid Id { get; set; }
        public required string Content { get; set; }
        public required List<AnswerDTO> Answers { get; set; }
    }
}
