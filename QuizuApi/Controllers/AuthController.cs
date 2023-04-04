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
            (string accessToken, string userId, string refreshToken) loginResponse;
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

            Response.Cookies.Append("refreshToken", loginResponse.refreshToken, options);

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new LoginResponseDTO() 
                {
                    AccessToken = loginResponse.accessToken,
                    UserId = loginResponse.userId
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
    }
}
