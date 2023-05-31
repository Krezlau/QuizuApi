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
    public class PlayController : ControllerBase
    {
        private readonly IQuestionAnswerRepository _qRepo;
        private readonly IQuizRepository _quizRepo;
        private readonly IRepository<QuizSettings> _settingsRepo;

        public PlayController(IQuizRepository quizRepo, IQuestionAnswerRepository qRepo, IRepository<QuizSettings> settingsRepo)
        {
            _quizRepo = quizRepo;
            _qRepo = qRepo;
            _settingsRepo = settingsRepo;
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
                    Questions = questions.Select(q => new QuestionDTO(q)).ToList(),
                    AnswerTimeS = answertime_s,
                    QuestionsCount = questionsPerPlay
                }
            });
        }
    }
}
