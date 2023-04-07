using Azure.Core;
using Microsoft.AspNetCore.Authorization;
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

            Quiz? quiz;
            try
            {
                quiz = await _quizRepo.GetAsync(q => q.Id == quizId, includeProperties: "Tags,Author,Settings");
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Something went wrong." }
                });
            }

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

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> CreateEmptyQuiz([FromBody] QuizCreateRequestDTO request)
        {
            var userId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (userId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user." }
                });
            }

            if (! await _quizRepo.CheckIfTitleAvailable(request.Title))
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Title not available." }
                });
            }

            var quiz = new Quiz()
            {
                AuthorId = userId,
                Title = request.Title,
            };
            try
            {
                await _quizRepo.CreateAsync(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Something went wrong." }
                });
            }

            return CreatedAtRoute(quiz.Id, new ApiResponse()
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
            });
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> DeleteQuiz(string id)
        {
            var userId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (userId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user id." }
                });
            }

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

            var quiz = await _quizRepo.GetAsync(q => q.Id == quizId);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            if (quiz.AuthorId != userId)
            {
                return Forbid();
            }

            try
            {
                await _quizRepo.DeleteAsync(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Something went wrong." }
                });
            }

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
            });
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateQuiz(string id, [FromBody]QuizUpdateRequestDTO request)
        {
            var userId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (userId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user id." }
                });
            }

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

            var quiz = await _quizRepo.GetAsync(q => q.Id == quizId);

            if (quiz is null)
            {
                return NotFound(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = { "Could not found quiz with the specified id." }
                });
            }

            if (quiz.AuthorId != userId)
            {
                return Forbid();
            }

            if (request.Title != quiz.Title)
            {
                if (await _quizRepo.CheckIfTitleAvailable(request.Title))
                {
                    quiz.Title = request.Title;
                }
                else
                {
                    return BadRequest(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = { "Title not available." }
                    });
                }
            }

            quiz.Description = request.Description;
            quiz.About = request.About;

            try
            {
                await _quizRepo.UpdateAsync(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Something went wrong." }
                });
            }

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            });
        }
    }
}
