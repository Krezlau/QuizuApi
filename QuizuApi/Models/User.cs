using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models
{
    public class User : IdentityUser
    {
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Name { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Surname { get; set; }
        [MinLength(Constraints.LabelLengthMin)]
        [MaxLength(Constraints.LabelLengthMax)]
        public required string Location { get; set; } = string.Empty;
        [MaxLength(Constraints.AboutLengthMax)]
        public string? About { get; set; } = string.Empty;
        public required DateTime JoinedAt { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<User> Followers { get; set; } = new List<User>();
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<User> Following { get; set; } = new List<User>();
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<Quiz> Quizzes { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizPlay> QuizPlays { get; set; }
        public virtual UserSettings Settings { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizLike> QuizLikes { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual List<QuizComment> QuizComments { get; set; }
        public virtual RefreshToken RefreshToken { get; set; }
    }
}
