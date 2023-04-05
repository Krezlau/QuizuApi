using Microsoft.EntityFrameworkCore;
using QuizuApi.Models.Database;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class QuizDTO
    {
        public QuizDTO()
        {
            
        }

        [SetsRequiredMembers]
        public QuizDTO(Quiz quiz, QuizActivityDTO activity)
        {
            Id = quiz.Id;
            Title = quiz.Title;
            Description = quiz.Description;
            AuthorName = quiz.Author.Name;
            AuthorId = quiz.AuthorId;
            LikesCount = activity.LikesCount;
            PlaysCount = activity.PlaysCount;
            TagNames = quiz.Tags.Select(t => t.Name).ToList();
            CommentsCount = activity.CommentsCount;
            IsLikedByUser = activity.IsLikedByUser;
            IsAlreadyPlayedByUser = activity.IsAlreadyPlayedByUser;
        }

        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string AuthorName { get; set; }
        public required string AuthorId { get; set; }
        public required int LikesCount { get; set; }
        public required int PlaysCount { get; set; }
        public List<string> TagNames { get; set; } = new List<string>();
        public required int CommentsCount { get; set; }
        public required bool IsLikedByUser { get; set; }
        public required bool IsAlreadyPlayedByUser { get; set; }
    }
}
