using Pharmacy_API.Dtos.Question;

namespace Pharmacy_API.Services.Question
{
    public interface IQAService
    {
        Task<List<QuestionDto>> GetQuestionsAsync(int productId, string sort);
        Task<int> CreateQuestionAsync(CreateQuestionRequest req);
        Task<int> CreateAnswerAsync(int questionId, CreateAnswerRequest req, string respondentName, string respondentRole);
        Task<bool> ToggleHelpfulAsync(int questionId);
        Task<bool> DeleteQuestionAsync(int questionId);
    }
}
