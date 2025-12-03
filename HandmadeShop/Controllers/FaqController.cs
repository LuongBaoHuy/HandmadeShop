using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeShop.Controllers
{
    public class FaqController : BaseController
    {
        private readonly ILogger<FaqController> _logger;

        public FaqController(HandmadeShopContext context, ILogger<FaqController> logger) : base(context)
        {
            _logger = logger;
        }

        // Hiển thị chat chung
        public IActionResult Faq()
        {
            var chatMessages = db.Questions
                .Where(q => q.ProductId == null)
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            ViewBag.ChatMessages = chatMessages;
            return View();
        }

        // Gửi tin nhắn chat chung (chỉ cho user đã đăng nhập)
        [HttpPost]
        public IActionResult SendChat(string message)
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(message))
                return Unauthorized();

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return Unauthorized();

            var chat = new Question
            {
                UserId = user.Id,
                Content = message,
                ProductId = null,
                CreatedAt = DateTime.Now,
                Status = "active"
            };
            db.Questions.Add(chat);
            db.SaveChanges();

            return RedirectToAction("Faq");
        }

        // Trả lời câu hỏi trong chat chung
        [HttpPost]
        public IActionResult Reply(int questionId, string content, int? parentAnswerId)
        {
            System.Diagnostics.Debug.WriteLine($"Reply: questionId={questionId}, parentAnswerId={parentAnswerId}, content={content}");

            if (!User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(content))
                return Unauthorized();

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return Unauthorized();

            var answer = new Answer
            {
                QuestionId = questionId,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.Now,
                ParentAnswerId = parentAnswerId // null nếu trả lời câu hỏi gốc, khác null nếu trả lời lồng nhau
            };
            db.Answers.Add(answer);
            db.SaveChanges();

            return RedirectToAction("Faq");
        }
    }
}
