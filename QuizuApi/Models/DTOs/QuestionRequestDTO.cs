using QuizuApi.Models.Database;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class QuestionRequestDTO
    {
        [SetsRequiredMembers]
        public QuestionRequestDTO(Question question)
        {
            Content = question.Content;
            Answers = question.Answers.Select(a => new AnswerRequestDTO(a)).ToList();
        }

        public QuestionRequestDTO()
        {

        }

        [MinLength(Constraints.QuestionLengthMin)]
        [MaxLength(Constraints.QuestionLengthMax)]
        public required string Content { get; set; }
        [MinLength(Constraints.AnswerListLengthMin)]
        [MaxLength(Constraints.AnswerListLengthMax)]
        public required List<AnswerRequestDTO> Answers { get; set; }
        public required string QuizId { get; set; }
    }
}
