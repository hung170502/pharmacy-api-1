using AutoMapper;
using Pharmacy_API.Dtos;
using Pharmacy_API.Dtos.Question;
using Pharmacy_API.Models;
using Pharmacy_API.Models.Question;
using Pharmacy_API.Repositories;
using Pharmacy_API.Repositories.Question;
namespace Pharmacy_API.Services.Question
{
    public class QAService : IQAService
    {
        private readonly IQARepository _repo;
        private readonly IMapper _mapper;

        public QAService(IQARepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<QuestionDto>> GetQuestionsAsync(int productId, string sort)
        {
            var questions = await _repo.GetByProductIdAsync(productId);

            // Tìm câu hỏi có vote cao nhất để gắn tag "helpful"
            var maxHelpful = questions.Any() ? questions.Max(q => q.HelpfulCount) : 0;

            // Sort
            var sorted = sort switch
            {
                "newest" => questions.OrderByDescending(q => q.CreatedAt),
                "oldest" => questions.OrderBy(q => q.CreatedAt),
                _ => questions.OrderByDescending(q => q.HelpfulCount), // "helpful" default
            };

            var result = sorted.Select(q =>
            {
                var dto = _mapper.Map<QuestionDto>(q);
                dto.Tag = (q.HelpfulCount == maxHelpful && maxHelpful > 0) ? "helpful" : null;
                return dto;
            }).ToList();

            return result;
        }

        public async Task<int> CreateQuestionAsync(CreateQuestionRequest req)
        {
            var question = new ProductQuestion
            {
                ProductId = req.ProductId,
                UserName = req.UserName.Trim(),
                Content = req.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            var saved = await _repo.AddQuestionAsync(question);
            return saved.QuestionId;
        }

        public async Task<int> CreateAnswerAsync(
            int questionId,
            CreateAnswerRequest req,
            string respondentName,
            string respondentRole)
        {
            var answer = new ProductAnswer
            {
                QuestionId = questionId,
                RespondentName = respondentName,
                RespondentRole = respondentRole,
                Content = req.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            var saved = await _repo.AddAnswerAsync(answer);
            return saved.AnswerId;
        }

        public async Task<bool> ToggleHelpfulAsync(int questionId)
            => await _repo.IncrementHelpfulAsync(questionId);

        public async Task<bool> DeleteQuestionAsync(int questionId)
            => await _repo.SoftDeleteQuestionAsync(questionId);
    }
}
