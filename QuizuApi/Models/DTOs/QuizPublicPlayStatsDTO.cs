namespace QuizuApi.Models.DTOs
{
    public class QuizPublicPlayStatsDTO
    {
        public int TotalPlays { get; set; }
        public int TopScore { get; set; }
        public double AverageScore { get; set; }
        public double AverageTimeS { get; set; }
        public int Step { get; set; }
        public List<BarPlotPointDTO> PlotPoints { get; set; }
    }
}
