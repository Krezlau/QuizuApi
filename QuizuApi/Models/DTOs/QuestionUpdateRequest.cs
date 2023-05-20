using QuizuApi.Models.Database;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class QuestionUpdateRequest
    {
        [SetsRequiredMembers]
        public QuestionUpdateRequest(Question question)
        {
            Content = question.Content;
            Answers = question.Answers.Select(a => new AnswerRequestDTO(a)).ToList();
        }

        public QuestionUpdateRequest()
        {

        }

        [MinLength(Constraints.QuestionLengthMin)]
        [MaxLength(Constraints.QuestionLengthMax)]
        public required string Content { get; set; }
        [MinLength(Constraints.AnswerListLengthMin)]
        [MaxLength(Constraints.AnswerListLengthMax)]
        public required List<AnswerRequestDTO> Answers { get; set; }
    }
}
