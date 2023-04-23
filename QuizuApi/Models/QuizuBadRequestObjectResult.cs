using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace QuizuApi.Models
{
    public class QuizuBadRequestObjectResult : BadRequestObjectResult
    {
        public QuizuBadRequestObjectResult([ActionResultObjectValue] object? error) : base(error)
        {

        }

        public QuizuBadRequestObjectResult(ApiResponse resp) : base(resp)
        {
            Formatters = null;
            Value = resp;
        }

        public List<string> ErrorMessages { get; set; }
    }
}
