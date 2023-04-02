using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models
{
    public class RefreshToken
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [MaxLength(Constraints.RefreshTokenLength)]
        public required string Value { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required bool IsRevoked { get; set; } = false;
        public required bool IsActive { get; set; } = false;
        [ForeignKey(nameof(UserId))]
        public required string UserId { get; set; }
        public virtual User User { get; set; }
    }
}
