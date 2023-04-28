using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccessTokenReaderService _tokenReader;

        public UsersController(IUserRepository userRepository, IAccessTokenReaderService tokenReader)
        {
            _userRepository = userRepository;
            _tokenReader = tokenReader;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetUser(string userId)
        {
            User? user;
            try
            {
                user = await _userRepository.GetAsync(u => u.Id == userId);
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

            if (user is null)
            {
                return NotFound(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find an user with corresponding id." }
                });
            }

            var ret = new UserProfileDTO(user,
                                         await _userRepository.FetchUserQuizCountAsync(userId),
                                         await _userRepository.FetchUserFollowersCount(userId));

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = ret
            });
        }

        [HttpGet("available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CheckUsernameAvailability([FromQuery] string username)
        {
            if (username is null || username.Length > Constraints.LabelLengthMax || username.Length < Constraints.LabelLengthMin)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid username." }
                });
            }

            var outcome = await _userRepository.CheckIfUsernameAvailable(username);

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = outcome
            });
        }
    }
}
