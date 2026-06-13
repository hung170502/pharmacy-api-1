namespace Pharmacy_API.Dtos.Question
{
    // --- Response DTOs ---
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int HelpfulCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? Tag { get; set; } // "helpful" nếu vote cao nhất
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int AnswerId { get; set; }
        public string RespondentName { get; set; } = string.Empty;
        public string RespondentRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    // --- Request DTOs ---
    public class CreateQuestionRequest
    {
        public int ProductId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class CreateAnswerRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
