using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace QuizuApi.Models
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            
        }

        [SetsRequiredMembers]
        public ApiResponse(ActionContext ac)
        {
            StatusCode = System.Net.HttpStatusCode.BadRequest;
            IsSuccess = false;
            ErrorMessages = new List<string>();
            foreach (var item in ac.ModelState) 
            {
                foreach (var error in item.Value.Errors)
                {
                    ErrorMessages.Add(error.ErrorMessage);
                }
            }
        }

        public required HttpStatusCode StatusCode { get; set; }
        public required bool IsSuccess { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public object? Result { get; set; }
    }
}
