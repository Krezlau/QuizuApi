using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizuApi.Models;
using QuizuApi.Models.Database;
using QuizuApi.Repository.IRepository;
using QuizuApi.Services;
using System.Net;

namespace QuizuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly IAccessTokenReaderService _tokenReader;
        private readonly IRepository<QuizLike> _quizLikeRepository;
        private readonly IQuizRepository _quizRepository;

        public LikesController(IRepository<QuizLike> quizLikeRepository, IAccessTokenReaderService tokenReader, IQuizRepository quizRepository)
        {
            _quizLikeRepository = quizLikeRepository;
            _tokenReader = tokenReader;
            _quizRepository = quizRepository;
        }

        [HttpPost("{id}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> LikeQuiz(string id)
        {
            var requestUserId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (requestUserId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = {"Invalid user token."}
                });
            }

            bool outcome = Guid.TryParseExact(id, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            Quiz? quiz = await _quizRepository.GetAsync(q => q.Id == quizGuid, includeProperties: "Likes");

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find quiz with specified id." }
                });
            }

            if (quiz.Likes.Any(l => l.UserId == requestUserId))
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz already liked by user." }
                });
            }

            try
            {
                await _quizLikeRepository.CreateAsync(new QuizLike() { QuizId = quizGuid, UserId = requestUserId });
                return Ok(new ApiResponse() { StatusCode = HttpStatusCode.OK, IsSuccess = true});
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

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> RemoveLikeFromQuiz(string id)
        {
            var requestUserId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (requestUserId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user token." }
                });
            }

            bool outcome = Guid.TryParseExact(id, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            Quiz? quiz = await _quizRepository.GetAsync(q => q.Id == quizGuid, includeProperties: "Likes");

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find quiz with specified id." }
                });
            }

            QuizLike? like = quiz.Likes.Where(l => l.UserId == requestUserId).FirstOrDefault();

            if (like is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz is not liked by user." }
                });
            }

            try
            {
                await _quizLikeRepository.DeleteAsync(like);
                return Ok(new ApiResponse() { StatusCode = HttpStatusCode.OK, IsSuccess = true });
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
