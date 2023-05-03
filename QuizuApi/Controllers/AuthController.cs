using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using QuizuApi.Data;
using QuizuApi.Exceptions;
using QuizuApi.Models;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using QuizuApi.Services;
using System.Net;

namespace QuizuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IAccessTokenReaderService _tokenService;

        public AuthController(IAuthRepository userRepository, IAccessTokenReaderService tokenService)
        {
            _authRepo = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequestDTO request)
        {
            (string accessToken, string userId, string refreshToken, string username) loginResponse;
            try
            {
                loginResponse = await _authRepo.LoginUserAsync(request);
            }
            catch (AuthException e)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { e.Message }
                });
            }

            var options = new CookieOptions();
            options.HttpOnly = true;
            options.Secure = true;
            options.Expires = DateTime.Now.AddYears(10);
            options.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            //options.Domain = "localhost";
            options.IsEssential = true;

            Response.Cookies.Append("refreshToken", loginResponse.refreshToken, options);

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new LoginResponseDTO() 
                {
                    AccessToken = loginResponse.accessToken,
                    UserId = loginResponse.userId,
                    Username = loginResponse.username
                }
            });
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequestDTO request)
        {
            bool ifUserNameUnique = await _authRepo.IsUniqueUserAsync(request.Username);
            if (!ifUserNameUnique)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Username already taken." }
                });
            }

            try
            {
                var user = await _authRepo.RegisterUserAsync(request);
                return Ok(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = user
                });
            }
            catch (AuthException e)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = e.Message.Split("\r\n").ToList()
                });
            }

        }
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Refresh([FromBody] RefreshRequestDTO request)
        {
            string? refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "No refresh token." }
                });
            }

            try
            {
                string token = await _authRepo.RefreshAsync(request.AccessToken, refreshToken);
                return Ok(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = token
                });
            }
            catch (AuthException e)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { e.Message }
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            string? userId = _tokenService.RetrieveUserIdFromRequest(Request);

            if (userId is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid access token." }
                });
            }

            bool outcome;
            try
            {
                outcome = await _authRepo.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            }
            catch (AuthException e)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = e.Message.Split("\r\n").ToList()
                });
            }
            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid access token" }
                });
            }
            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = "Successfully changed the password."
            });
        }
    }
}
