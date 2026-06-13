// Controllers/QAController.cs
using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Question;
using Pharmacy_API.Services.Question;

namespace Pharmacy_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QAController : ControllerBase
    {
        private readonly IQAService _qaService;

        public QAController(IQAService qaService)
        {
            _qaService = qaService;
        }

        /// <summary>
        /// Lấy danh sách câu hỏi theo productId
        /// </summary>
        /// <param name="productId">ID sản phẩm</param>
        /// <param name="sort">Cách sắp xếp: helpful, newest, oldest</param>
        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions([FromQuery] int productId, [FromQuery] string sort = "helpful")
        {
            var questions = await _qaService.GetQuestionsAsync(productId, sort);
            return Ok(new { success = true, data = questions });
        }

        /// <summary>
        /// Tạo câu hỏi mới
        /// </summary>
        [HttpPost("questions")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                var questionId = await _qaService.CreateQuestionAsync(request);
                return Ok(new { success = true, message = "Question created successfully", questionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo câu trả lời cho câu hỏi (dành cho admin/dược sĩ)
        /// </summary>
        [HttpPost("questions/{questionId}/answers")]
        public async Task<IActionResult> CreateAnswer(
            int questionId,
            [FromBody] CreateAnswerRequest request,
            [FromQuery] string respondentName = "Pharmacist",
            [FromQuery] string respondentRole = "pharmacist")
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                var answerId = await _qaService.CreateAnswerAsync(questionId, request, respondentName, respondentRole);
                return Ok(new { success = true, message = "Answer created successfully", answerId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu câu hỏi là hữu ích
        /// </summary>
        [HttpPost("questions/{questionId}/helpful")]
        public async Task<IActionResult> ToggleHelpful(int questionId)
        {
            var result = await _qaService.ToggleHelpfulAsync(questionId);
            if (!result)
                return NotFound(new { success = false, message = "Question not found" });

            return Ok(new { success = true, message = "Marked as helpful" });
        }

        /// <summary>
        /// Xóa câu hỏi (soft delete)
        /// </summary>
        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _qaService.DeleteQuestionAsync(questionId);
            if (!result)
                return NotFound(new { success = false, message = "Question not found" });

            return Ok(new { success = true, message = "Question deleted successfully" });
        }
    }
}