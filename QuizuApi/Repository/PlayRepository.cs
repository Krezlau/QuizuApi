using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Exceptions;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;
using System.ComponentModel;
using System.Linq.Expressions;

namespace QuizuApi.Repository
{
    public class PlayRepository : Repository<QuizPlay>, IPlayRepository
    {
        public PlayRepository(QuizuApiDbContext context) : base(context)
        {
        }

        public async Task<(double correctPercentage, double avgTimeTaken)> GetAverageScoreForQuestionAsync(Guid answerId)
        {
            Answer? ans = await _context.Answers.Where(a => a.Id == answerId).Include(a => a.Question).FirstOrDefaultAsync();

            if (ans is null)
            {
                throw new QuizPlayException("Answer doesn't exist.");
            }
            if (ans.Question is null)
            {
                throw new QuizPlayException("Questions doesn't exist.");
            }
            double avgTime = await _context.UserAnswers.Where(ua => ua.QuestionId == ans.QuestionId).AverageAsync(ua => ua.TimeTaken.TotalSeconds);
            Guid correctAnswerId;
            if (ans.IsCorrect)
            {
                correctAnswerId = ans.Id;
            }
            else
            {
                Answer? a = await _context.Answers.Where(a => a.QuestionId == ans.QuestionId && a.IsCorrect).FirstOrDefaultAsync();
                if (a is null)
                {
                    throw new QuizPlayException("Something wrong.");
                }
                correctAnswerId = a.Id;
            }
            int anscount = await _context.UserAnswers.Where(ua => ua.QuestionId == ans.QuestionId).CountAsync();
            double correctPercentage;
            if (anscount != 0)
            {
                correctPercentage = await _context.UserAnswers.Where(ua => ua.AnswerGivenId == correctAnswerId).CountAsync()
                                        / anscount * 100;
            }
            else
            {
                correctPercentage = 0;
            }

            return (correctPercentage, avgTime);
        }

        public async Task<(double avgScore, double avgTimeTaken)> GetAverageScoreForQuizAsync(Guid quizId)
        {
            Quiz? quiz = await _context.Quizzes.Where(q => q.Id == quizId).FirstOrDefaultAsync();

            if (quiz is null)
            {
                throw new QuizPlayException("Quiz does not exist.");
            }

            double avgScore = await _context.QuizPlays.Where(qp => qp.QuizId == quizId).AverageAsync(qp => qp.Score);
            double avgTimeTaken = await _context.QuizPlays.Where(qp => qp.QuizId == quizId)
                                                          .Include(qp => qp.Answers)
                                                          .AverageAsync(qp => qp.Answers.Average(a => a.TimeTaken.TotalSeconds));
            return (avgScore, avgTimeTaken);
        }

        public async Task<double> GetPercentageOfUsersYouBeatAsync(Guid quizId, int score)
        {
            Quiz? quiz = await _context.Quizzes.Where(q => q.Id == quizId).FirstOrDefaultAsync();

            if (quiz is null)
            {
                throw new QuizPlayException("Quiz does not exist.");
            }

            int all = await _context.QuizPlays.Where(qp => qp.QuizId == quizId).CountAsync();
            if (all == 0) return 100;

            return await _context.QuizPlays.Where(qp => qp.QuizId == quizId && qp.Score <= score).CountAsync() / all * 100;
        }

        public async Task SavePlayAsync(string userId, Guid quizId, UserPlayResultDTO answers)
        {
            // validate answers 
            if (answers.AnswerIds.Count != answers.TimeTookS.Count)
            {
                throw new QuizPlayException("Something is wrong with answers.");
            }
            var userAns = new List<UserAnswer>();
            for (int i = 0; i < answers.AnswerIds.Count; i++)
            {
                bool outcome = Guid.TryParseExact(answers.AnswerIds[i], "D", out Guid guid);
                if (!outcome)
                {
                    throw new QuizPlayException("Answer doesn't exist.");
                }
                Answer? ans = await _context.Answers.Where(a => a.Id == guid).Include(q => q.Question).FirstOrDefaultAsync();
                if (ans is null)
                {
                    throw new QuizPlayException("Answer doesn't exist.");
                }
                if (ans.Question is null)
                {
                    throw new QuizPlayException("Question doesn't exist.");
                }
                if (ans.Question.QuizId != quizId)
                {
                    throw new QuizPlayException("Wrong quiz.");
                }

                userAns.Add(new UserAnswer()
                {
                    AnswerGivenId = guid,
                    QuestionId = ans.QuestionId,
                    TimeTaken = TimeSpan.FromSeconds(answers.TimeTookS[i])
                });

            }

            var play = new QuizPlay()
            {
                QuizId = quizId,
                Answers = userAns,
                Score = answers.Score,
                UserId = userId
            };
            
            await _context.QuizPlays.AddAsync(play);
            await _context.SaveChangesAsync();
        }
    }
}
