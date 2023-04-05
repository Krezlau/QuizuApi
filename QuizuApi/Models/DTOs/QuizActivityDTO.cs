namespace QuizuApi.Models.DTOs
{
    public class QuizActivityDTO
    {
        public required int LikesCount { get; set; }
        public required int PlaysCount { get; set; }
        public required int CommentsCount { get; set; }
        public bool IsLikedByUser { get; set; } = false;
        public bool IsAlreadyPlayedByUser { get; set; } = false;
    }
}
