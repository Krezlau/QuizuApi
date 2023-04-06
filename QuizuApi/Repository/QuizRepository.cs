﻿using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models.Database;
using QuizuApi.Models.DTOs;
using QuizuApi.Repository.IRepository;

namespace QuizuApi.Repository
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(QuizuApiDbContext context) : base(context)
        {
        }

        public async Task<QuizActivityDTO> FetchActivityInfoAsync(Guid quizId, string? userId = null)
        {
            var result = new QuizActivityDTO()
            {
                LikesCount = await _context.QuizLikes.CountAsync(l => l.QuizId == quizId),
                CommentsCount = await _context.QuizComments.CountAsync(c => c.QuizId == quizId),
                PlaysCount = await _context.QuizPlays.CountAsync(p => p.QuizId == quizId)
            };

            if (userId is not null)
            {
                result.IsLikedByUser = await _context.QuizLikes.AnyAsync(l => l.QuizId == quizId && l.UserId == userId);
                result.IsAlreadyPlayedByUser = await _context.QuizPlays.AnyAsync(p => p.QuizId == quizId && p.UserId == userId);
            }
            return result;
        }

        public override async Task CreateAsync(Quiz entity)
        {
            await base.CreateAsync(entity);
            _context.QuizSettings.Add(new QuizSettings()
            {
                AllowReplays = true,
                AnswerTimeS = 10,
                QuestionsPerPlay = -1, // all questions
                QuizId = entity.Id,
            });
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Quiz entity)
        {
            var questions = await _context.Questions.Where(q => q.QuizId == entity.Id).ToArrayAsync();
            foreach (var q in questions)
            {
                _context.Answers.RemoveRange(await _context.Answers.Where(a => a.QuestionId == q.Id).ToArrayAsync());
            }
            _context.Questions.RemoveRange(questions);
            _context.QuizLikes.RemoveRange(await _context.QuizLikes.Where(ql => ql.QuizId == entity.Id).ToArrayAsync());
            _context.QuizComments.RemoveRange(await _context.QuizComments.Where(qc => qc.QuizId == entity.Id).ToArrayAsync());
            _context.QuizPlays.RemoveRange(await _context.QuizPlays.Where(qp => qp.QuizId == entity.Id).ToArrayAsync());
            await base.DeleteAsync(entity);
        }
    }
}