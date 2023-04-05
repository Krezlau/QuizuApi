using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizuApi.Models;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using QuizuApi.Services;
using System.Net;

namespace QuizuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizRepository _quizRepo;
        private readonly IAccessTokenReaderService _tokenReader;

        public QuizzesController(IQuizRepository quizRepo, IAccessTokenReaderService tokenReader)
        {
            _quizRepo = quizRepo;
            _tokenReader = tokenReader;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetQuizzes([FromQuery] PageRequestParametersDTO request)
        {
            var userId = _tokenReader.RetrieveUserIdFromRequest(Request);
            var quizResults = await _quizRepo.GetPageAsync(request.PageNumber, request.PageSize, includeProperties: "Tags,Author");
            var retResult = new List<QuizDTO>();
            foreach (var quiz in quizResults.QueryResult)
            {
                retResult.Add(new QuizDTO(quiz, await _quizRepo.FetchActivityInfoAsync(quiz.Id, userId)));
            }

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new PageResultDTO<QuizDTO>()
                {
                    PageCount = quizResults.PageCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    QueryResult = retResult
                }
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetQuizInfo(string id)
        {
            var userId = _tokenReader.RetrieveUserIdFromRequest(Request);

            bool outcome = Guid.TryParseExact(id, "D", out Guid quizId);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            var quiz = await _quizRepo.GetAsync(q => q.Id == quizId, includeProperties: "Tags,Author,Settings");

            if (quiz is null)
            {
                return NotFound(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find a quiz with corresponding id." }
                });
            }

            var ret = new QuizDetailsDTO(quiz, await _quizRepo.FetchActivityInfoAsync(quizId, userId));

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = ret
            });
        }
    }
}
