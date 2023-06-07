using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizuApi.Exceptions;
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
    public class PlayController : ControllerBase
    {
        private readonly IQuestionAnswerRepository _qRepo;
        private readonly IQuizRepository _quizRepo;
        private readonly IRepository<QuizSettings> _settingsRepo;
        private readonly IAccessTokenReaderService _tokenReader;
        private readonly IPlayRepository _playRepo;

        public PlayController(IQuizRepository quizRepo, IQuestionAnswerRepository qRepo, IRepository<QuizSettings> settingsRepo, IAccessTokenReaderService tokenReader, IPlayRepository playRepo)
        {
            _quizRepo = quizRepo;
            _qRepo = qRepo;
            _settingsRepo = settingsRepo;
            _tokenReader = tokenReader;
            _playRepo = playRepo;
        }

        [HttpGet("{quizId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetQuizQuestions(string quizId)
        {
            bool outcome = Guid.TryParseExact(quizId, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            // check if quiz exists
            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == quizGuid);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz does not exist." }
                });
            }

            var quizName = quiz.Title;

            // get quiz settings
            QuizSettings? quizSettings = await _settingsRepo.GetAsync(s => s.QuizId == quizGuid);

            var answertime_s = quizSettings is null ? 10 : quizSettings.AnswerTimeS;
            var questionsPerPlay = quizSettings is null ? 10 : quizSettings.QuestionsPerPlay;
            
            // get questions and answers
            List<Question> questions = await _qRepo.GetRandomQuestionsForPlayAsync(quizGuid, questionsPerPlay);

            if (questions.Count == 0)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz does not have any questions." }
                });
            }
            // parse to DTO

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new PlayQuestionsDTO()
                {
                    QuizId = quizId,
                    QuizName = quizName,
                    Questions = questions.Select(q => new QuestionDTO(q)).ToList(),
                    AnswerTimeS = answertime_s,
                    QuestionsCount = questionsPerPlay
                }
            });
        }

        [HttpPost("{quizId}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> SubmitScore(string quizId, [FromBody] UserPlayResultDTO answers)
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

            bool outcome = Guid.TryParseExact(quizId, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            // check if quiz exists
            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == quizGuid);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz does not exist." }
                });
            }

            double percentage;
            try
            {
                percentage = await _playRepo.GetPercentageOfUsersYouBeatAsync(quizGuid, answers.Score);
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

            try
            {
                await _playRepo.SavePlayAsync(requestUserId, quizGuid, answers);
            }
            catch (QuizPlayException ex)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { ex.Message }
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

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = percentage
            });
        }

        [HttpGet("stats/{quizId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetQuizPublicPlayStats(string quizId)
        {
            bool outcome = Guid.TryParseExact(quizId, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            // check if quiz exists
            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == quizGuid, includeProperties:"Questions");

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Quiz does not exist." }
                });
            }

            QuizSettings? quizSettings = await _settingsRepo.GetAsync(s => s.QuizId == quizGuid);

            var questionsPerPlay = quizSettings is null ? 10 : quizSettings.QuestionsPerPlay == -1 ? quiz.Questions.Count : quizSettings.QuestionsPerPlay;

            var stats = await _playRepo.GetQuizPublicPlayStatsAsync(quizGuid, questionsPerPlay);

            return Ok(new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = stats
            });
        }
    }
}
