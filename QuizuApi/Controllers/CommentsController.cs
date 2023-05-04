using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizuApi.Models.DTOs;
using QuizuApi.Models;
using System.Net;
using QuizuApi.Repository.IRepository;
using QuizuApi.Models.Database;
using QuizuApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace QuizuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IRepository<QuizComment> _commentRepo;
        private readonly IAccessTokenReaderService _tokenReader;

        public CommentsController(IAccessTokenReaderService tokenReader, IQuizRepository quizRepo, IRepository<QuizComment> commentRepo)
        {
            _tokenReader = tokenReader;
            _commentRepo = commentRepo;
        }

        [HttpGet("quiz/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetComments(string id, [FromQuery] PageRequestParametersDTO request)
        {
            var commentsResults = await _commentRepo.GetPageAsync(request.PageNumber, request.PageSize, includeProperties: "Author");

            var retResult = commentsResults.QueryResult.Select(c => new QuizCommentDTO()
            {
                Id = c.Id,
                AuthorId = c.AuthorId,
                AuthorName = c.Author.UserName is not null ? c.Author.UserName : "[deleted]",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new PageResultDTO<QuizCommentDTO>()
                {
                    PageCount = commentsResults.PageCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    QueryResult = retResult
                }
            });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> CreateNewComment([FromBody] QuizCommentRequestDTO request)
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

            var comment = new QuizComment()
            {
                Content = request.Content,
                AuthorId = userId,
                QuizId = request.QuizId,
            };
            try
            {
                await _commentRepo.CreateAsync(comment);
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

            return CreatedAtRoute(comment.Id, new ApiResponse()
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
                Result = comment.Id
            });
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> DeleteQuizComment(string id)
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

            bool outcome = Guid.TryParseExact(id, "D", out Guid guid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            QuizComment? comment = await _commentRepo.GetAsync(c => c.Id == guid);

            if (comment is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find comment with specified id." }
                });
            }

            if (comment.AuthorId != userId)
            {
                return Forbid();
            }

            try
            {
                await _commentRepo.DeleteAsync(comment);
                return Ok(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                });
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
        }
    }
}
