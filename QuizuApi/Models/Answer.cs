using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class Answer : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        [MinLength(Constraints.AnswerLengthMin)]
        [MaxLength(Constraints.AnswerLengthMax)]
        public required string Content { get; set; }
        public required bool IsCorrect { get; set; }
        public virtual Question Question { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public required Guid QuestionId { get; set; }
    }
}
