namespace QuizuApi.Models.DTOs
{
    public class PlayQuestionsDTO
    {
        public required string QuizName { get; set; }
        public required int QuestionsCount { get; set; }
        public required int AnswerTimeS { get; set; }
        public required List<QuestionDTO> Questions { get; set;}
    }
}
