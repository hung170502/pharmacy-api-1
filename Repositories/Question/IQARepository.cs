using Pharmacy_API.Models.Question;

namespace Pharmacy_API.Repositories.Question
{
    public interface IQARepository
    {
        Task<List<ProductQuestion>> GetByProductIdAsync(int productId);
        Task<ProductQuestion?> GetByIdAsync(int questionId);
        Task<ProductQuestion> AddQuestionAsync(ProductQuestion question);
        Task<ProductAnswer> AddAnswerAsync(ProductAnswer answer);
        Task<bool> IncrementHelpfulAsync(int questionId);
        Task<bool> SoftDeleteQuestionAsync(int questionId);
    }
}
