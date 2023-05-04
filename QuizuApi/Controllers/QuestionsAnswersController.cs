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
    public class QuestionsAnswersController : ControllerBase
    {
        private readonly IQuestionAnswerRepository _qRepo;
        private readonly IAccessTokenReaderService _tokenReader;
        private readonly IQuizRepository _quizRepo;

        public QuestionsAnswersController(IQuizRepository quizRepo, IAccessTokenReaderService tokenReader, IQuestionAnswerRepository qRepo)
        {
            _quizRepo = quizRepo;
            _tokenReader = tokenReader;
            _qRepo = qRepo;
        }

        [HttpGet("quiz/{quizId}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> GetQuestionsAndAnswersForQuiz(string quizId)
        {
            var requestUserId = _tokenReader.RetrieveUserIdFromRequest(Request);

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

            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == quizGuid);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find the quiz with specified id." }
                });
            }

            if (quiz.AuthorId != requestUserId)
            {
                return Forbid();
            }
            List<QuestionDTO> ret;
            try
            {
                var questions = await _qRepo.GetAllAsync(quizGuid);
                ret = questions.Select(q => new QuestionDTO(q)).ToList();
            }
            catch (Exception e)
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
                Result = ret
            });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> CreateQuestion([FromBody] QuestionRequestDTO question)
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

            bool outcome = Guid.TryParseExact(question.QuizId, "D", out Guid quizGuid);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid quiz id." }
                });
            }

            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == quizGuid);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid quiz id." }
                });
            }

            if (quiz.AuthorId != userId)
            {
                return Forbid();
            }

            // assert that only one answer is correct
            if (question.Answers.Count(a => a.IsCorrect == true) != 1)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Exactly one answer needs to be correct." }
                });
            }

            try
            {
                Question q = await _qRepo.CreateAsync(question);
                return CreatedAtRoute(q.Id, new ApiResponse()
                {
                    StatusCode = HttpStatusCode.Created,
                    IsSuccess = true,
                    Result = q.Id
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Invalid quiz id.")
                {
                    return BadRequest(new ApiResponse()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = { ex.Message }
                    });
                }
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse>> DeleteQuestion(string id)
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

            bool outcome = Guid.TryParseExact(id, "D", out Guid questionId);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            Question? question = await _qRepo.GetAsync(questionId);

            if (question is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find question with specified id." }
                });
            }

            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == question.QuizId);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid quiz." }
                });
            }

            if (quiz.AuthorId != userId)
            {
                return Forbid();
            }

            try
            {
                await _qRepo.DeleteAsync(question);
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
        public async Task<ActionResult<ApiResponse>> UpdateQuestion([FromBody] QuestionUpdateRequest questionRequest, string id)
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

            bool outcome = Guid.TryParseExact(id, "D", out Guid questionId);

            if (!outcome)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid id." }
                });
            }

            Question? question = await _qRepo.GetAsync(questionId);

            if (question is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Could not find question with specified id." }
                });
            }

            Quiz? quiz = await _quizRepo.GetAsync(q => q.Id == question.QuizId);

            if (quiz is null)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = { "Invalid quiz id." }
                });
            }

            if (quiz.AuthorId != userId)
            {
                return Forbid();
            }

            try
            {
                // case 0: len equal
                if (question.Answers.Count == questionRequest.Answers.Count)
                {
                    for (int i = 0; i <  question.Answers.Count; i++)
                    {
                        question.Answers[i].Content = questionRequest.Answers[i].Content;
                        question.Answers[i].IsCorrect = questionRequest.Answers[i].IsCorrect;
                    }
                    question.Content = questionRequest.Content;

                    if (question.Answers.Count(a => a.IsCorrect == true) != 1)
                    {
                        return BadRequest(new ApiResponse()
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            IsSuccess = false,
                            ErrorMessages = { "Exactly one answer needs to be correct." }
                        });
                    }

                    await _qRepo.UpdateAsync(question);
                }

                // case 1: db len smaller -> need to add answer (if no more than 4)
                if (question.Answers.Count < questionRequest.Answers.Count)
                {
                    int i;
                    for (i = 0; i < question.Answers.Count; i++)
                    {
                        question.Answers[i].Content = questionRequest.Answers[i].Content;
                        question.Answers[i].IsCorrect = questionRequest.Answers[i].IsCorrect;
                    }

                    if (questionRequest.Answers.Count(a => a.IsCorrect == true) != 1)
                    {
                        return BadRequest(new ApiResponse()
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            IsSuccess = false,
                            ErrorMessages = { "Exactly one answer needs to be correct." }
                        });
                    }

                    while (i < questionRequest.Answers.Count)
                    {
                        await _qRepo.CreateAnswerAsync(questionRequest.Answers[i++], questionId);
                    }
                    question.Content = questionRequest.Content;

                    await _qRepo.UpdateAsync(question);
                }
                // case 2: db len larger -> need to delete answer (if no less than 1)
                if (question.Answers.Count > questionRequest.Answers.Count)
                {
                    int i;
                    for (i = 0; i < questionRequest.Answers.Count; i++)
                    {
                        question.Answers[i].Content = questionRequest.Answers[i].Content;
                        question.Answers[i].IsCorrect = questionRequest.Answers[i].IsCorrect;
                    }

                    if (questionRequest.Answers.Count(a => a.IsCorrect == true) != 1)
                    {
                        return BadRequest(new ApiResponse()
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            IsSuccess = false,
                            ErrorMessages = { "Exactly one answer needs to be correct." }
                        });
                    }

                    while (i < question.Answers.Count)
                    {
                        await _qRepo.DeleteAnswerAsync(question.Answers[i++]);
                    }
                    question.Content = questionRequest.Content;

                    await _qRepo.UpdateAsync(question);
                }

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
