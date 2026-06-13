// Repositories/QARepository.cs
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;   // thay bằng namespace DbContext của bạn
using Pharmacy_API.Models;
using Pharmacy_API.Models.Question;
using Pharmacy_API.Repositories.Question;

namespace Pharmacy_API.Repositories
{
    public class QARepository : IQARepository
    {
        private readonly AppDbContext _db; // thay AppDbContext bằng tên DbContext thật của bạn

        public QARepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProductQuestion>> GetByProductIdAsync(int productId)
        {
            return await _db.ProductQuestions
                .Where(q => q.ProductId == productId && q.IsActive)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductQuestion?> GetByIdAsync(int questionId)
        {
            return await _db.ProductQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.IsActive);
        }

        public async Task<ProductQuestion> AddQuestionAsync(ProductQuestion question)
        {
            _db.ProductQuestions.Add(question);
            await _db.SaveChangesAsync();
            return question;
        }

        public async Task<ProductAnswer> AddAnswerAsync(ProductAnswer answer)
        {
            _db.ProductAnswers.Add(answer);
            await _db.SaveChangesAsync();
            return answer;
        }

        public async Task<bool> IncrementHelpfulAsync(int questionId)
        {
            var question = await _db.ProductQuestions.FindAsync(questionId);
            if (question == null) return false;

            question.HelpfulCount++;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteQuestionAsync(int questionId)
        {
            var question = await _db.ProductQuestions.FindAsync(questionId);
            if (question == null) return false;

            question.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}