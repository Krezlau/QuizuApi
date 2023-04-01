using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class UserAnswer : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public virtual Question Question { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public required Guid QuestionId { get; set; }
        public virtual Answer AnswerGiven { get; set; }
        [ForeignKey(nameof(AnswerGivenId))]
        public required Guid AnswerGivenId { get; set; }
        public required TimeSpan TimeTaken { get; set; }
    }
}
