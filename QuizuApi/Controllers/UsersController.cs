using Microsoft.AspNetCore.Authorization;
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


        [HttpDelete("{userId}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> DeleteUserAccount(string userId)
        {
            var requestUserId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (requestUserId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user id." }
                });
            }

            var user = await _userRepository.GetAsync(u => u.Id == userId);

            if (user is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            if (user.Id != requestUserId)
            {
                return Forbid();
            }

            try
            {
                await _userRepository.DeleteAsync(user);
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

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponse>> UpdateUserProfile(string userId, [FromBody] UserProfileUpdateDTO request)
        {
            var requestUserId = _tokenReader.RetrieveUserIdFromRequest(Request);

            if (requestUserId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid user id." }
                });
            }

            var user = await _userRepository.GetAsync(u => u.Id == userId);

            if (user is null)
            {
                return NotFound(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = { "Could not found quiz with the specified id." }
                });
            }

            if (user.Id != requestUserId)
            {
                return Forbid();
            }

            if (request.Username != user.UserName)
            {
                if (await _userRepository.CheckIfUsernameAvailable(request.Username))
                {
                    user.UserName = request.Username;
                }
                else
                {
                    return BadRequest(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = { "Username not available." }
                    });
                }
            }

            user.About = request.About is not null ? request.About : user.About;
            user.Name = request.Name is not null ? request.Name : user.Name;
            user.Surname = request.Surname is not null ? request.Surname : user.Surname;
            user.Location = request.Location is not null ? request.Location : user.Location;

            try
            {
                await _userRepository.UpdateAsync(user);
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
