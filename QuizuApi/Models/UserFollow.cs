using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class UserFollow : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual User UserFollowing { get; set; }
        [ForeignKey(nameof(UserFollowingId))]
        public required string UserFollowingId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual User UserFollowed { get; set; }
        [ForeignKey(nameof(UserFollowedId))]
        public required string UserFollowedId { get; set; }
    }
}
