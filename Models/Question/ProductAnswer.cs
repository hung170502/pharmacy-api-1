namespace Pharmacy_API.Models.Question
{
    public class ProductAnswer
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public string RespondentName { get; set; } = string.Empty;
        public string RespondentRole { get; set; } = "pharmacist"; // "admin" | "pharmacist"
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ProductQuestion? Question { get; set; }
    }
}
