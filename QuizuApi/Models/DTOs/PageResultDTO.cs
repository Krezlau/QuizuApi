namespace QuizuApi.Models.DTOs
{
    public class PageResultDTO<T> where T : class
    {
        public required int PageNumber { get; set; }
        public required int PageCount { get; set; }
        public required int PageSize { get; set; }
        public required List<T> QueryResult { get; set; } = new List<T>();
    }
}
