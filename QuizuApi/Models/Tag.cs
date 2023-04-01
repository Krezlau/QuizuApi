using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class Tag : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Name { get; set; }
        public virtual List<Quiz> Quizzes { get; set; }
    }
}
