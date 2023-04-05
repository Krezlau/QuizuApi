using QuizuApi.Models.Database;
using System.Diagnostics.CodeAnalysis;

namespace QuizuApi.Models.DTOs
{
    public class QuizDetailsDTO : QuizDTO
    {
        public QuizDetailsDTO() { }

        [SetsRequiredMembers]
        public QuizDetailsDTO(Quiz quiz, QuizActivityDTO activity) : base(quiz, activity)
        {
            About = quiz.About;
            IsPlayAllowed = !activity.IsAlreadyPlayedByUser || quiz.Settings.AllowReplays;
            QuestionsPerPlay = quiz.Settings.QuestionsPerPlay;
            AnswerTimeS = quiz.Settings.AnswerTimeS;
        }

        public required bool IsPlayAllowed { get; set; }
        public string? About { get; set; } = null;
        public required int QuestionsPerPlay { get; set; }
        public required int AnswerTimeS { get; set; }

        // Some stats here
    }
}
