using System.ComponentModel.DataAnnotations;

namespace QuizuApi.Models.DTOs
{
    public class QuizCommentRequestDTO
    {
        [MinLength(Constraints.CommentLengthMin)]
        [MaxLength(Constraints.CommentLengthMax)]
        public required string Content { get; set; }
        public required Guid QuizId { get; set; }
    }
}
