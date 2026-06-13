namespace Pharmacy_API.Models.Question
{
    public class ProductQuestion
    {
        public int QuestionId { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int HelpfulCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<ProductAnswer> Answers { get; set; } = new();
    }
}
