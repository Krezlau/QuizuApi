using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class Question : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        [MinLength(Constraints.QuestionLengthMin)]
        [MaxLength(Constraints.QuestionLengthMax)]
        public required string Content { get; set; }
        [MinLength(Constraints.AnswerListLengthMin)]
        [MaxLength(Constraints.AnswerListLengthMax)]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<Answer> Answers { get; set; }
        public virtual Quiz Quiz { get; set; }
        public required Guid QuizId { get; set; }
    }
}
