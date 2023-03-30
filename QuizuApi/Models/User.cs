using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models
{
    public class User : IdentityUser
    {
        [MinLength(3)]
        [MaxLength(50)]
        public required string Name { get; set; }
        [MinLength(3)]
        [MaxLength(50)]
        public required string Surname { get; set; }
        [MinLength(3)]
        [MaxLength(50)]
        public required string Location { get; set; } = string.Empty;
        [MaxLength(1000)]
        public required string About { get; set; } = string.Empty;
        public required DateTime JoinedAt { get; set; }
        public virtual List<User> Followers { get; set; } = new List<User>();
    }
}
