using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class QuizSettings : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public virtual Quiz Quiz { get; set; }
        [ForeignKey(nameof(QuizId))]
        public required Guid QuizId { get; set; }
        public required int AnswerTimeS { get; set; }
        public required bool AllowReplays { get; set; }
        public required int QuestionsPerPlay { get; set; }
    }
}
