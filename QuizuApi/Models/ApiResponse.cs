using System.Net;

namespace QuizuApi.Models
{
    public class ApiResponse
    {
        public required HttpStatusCode StatusCode { get; set; }
        public required bool IsSuccess { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public object? Result { get; set; }
    }
}
