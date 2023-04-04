using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models.Database
{
    public class QuizComment : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public virtual User Author { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public required string AuthorId { get; set; }
        public virtual Quiz Quiz { get; set; }
        [ForeignKey(nameof(QuizId))]
        public required Guid QuizId { get; set; }
        [MinLength(Constraints.CommentLengthMin)]
        [MaxLength(Constraints.CommentLengthMax)]
        public required string Content { get; set; }
    }
}
