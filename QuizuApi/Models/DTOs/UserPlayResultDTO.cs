namespace QuizuApi.Models.DTOs
{
    public class UserPlayResultDTO
    {
        public int Score { get; set; }
        public List<string> AnswerIds { get; set; }
        public List<double> TimeTookS { get; set; }
        public List<string> QuestionIds { get; set; }
    }
}
