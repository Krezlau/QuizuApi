using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class Quiz : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        [MinLength(Constraints.TitleLengthMin)]
        [MaxLength(Constraints.TitleLengthMax)]
        public required string Title { get; set; }
        [MaxLength(Constraints.DescriptionLengthMax)]
        public string? Description { get; set; }
        [MaxLength(Constraints.AboutLengthMax)]
        public string? About { get; set; }
        public virtual User Author { get; set; }
        public required string AuthorId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<Question> Questions { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizLike> Likes { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizPlay> Plays { get; set; }
        [MaxLength(Constraints.TagsListLengthMax)]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<Tag> Tags { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizComment> Comments { get; set; }
        public virtual QuizSettings Settings { get; set; }
    }
}
