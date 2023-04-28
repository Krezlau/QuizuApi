using QuizuApi.Models.Database;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class UserProfileDTO
    {
        [SetsRequiredMembers]
        public UserProfileDTO(User user, int quizzesCount, int followersCount)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email is null ? "email invalid" : user.Email;
            Surname = user.Surname;
            JoinedAt = user.JoinedAt;
            Location = user.Location;
            About = user.About;
            QuizzesCount = quizzesCount;
            FollowersCount = followersCount;
        }

        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Surname { get; set; }
        public required DateTime JoinedAt { get; set; }
        public required string Location { get; set; }
        public required int QuizzesCount { get; set; }
        public required int FollowersCount { get; set; }
        public string? About { get; set; }
    }
}
