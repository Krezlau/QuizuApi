using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class QuizPlay : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        public virtual User User { get; set; }
        [ForeignKey(nameof(UserId))]
        public required string UserId { get; set; }
        public virtual Quiz Quiz { get; set; }
        [ForeignKey(nameof(QuizId))]
        public required Guid QuizId { get; set; }
        public virtual List<UserAnswer> Answers { get; set; }
    }
}
