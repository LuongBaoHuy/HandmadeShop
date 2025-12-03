using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class QuestionsController : Controller
    {
        private readonly Data.HandmadeShopContext _context;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(Data.HandmadeShopContext context, ILogger<QuestionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Trang danh sách câu hỏi
        public async Task<IActionResult> Index(string searchTerm, string status, string sortBy = "createdat", string sortOrder = "desc")
        {
            try
            {
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Status = status;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;

                // Chỉ lấy dữ liệu thô từ bảng Questions
                var questionsQuery = _context.Questions.AsNoTracking();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    questionsQuery = questionsQuery.Where(q => q.Content.Contains(searchTerm));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    questionsQuery = questionsQuery.Where(q => q.Status == status);
                }
                switch (sortBy?.ToLower())
                {
                    case "content":
                        questionsQuery = sortOrder == "desc" ? questionsQuery.OrderByDescending(q => q.Content) : questionsQuery.OrderBy(q => q.Content);
                        break;
                    case "status":
                        questionsQuery = sortOrder == "desc" ? questionsQuery.OrderByDescending(q => q.Status) : questionsQuery.OrderBy(q => q.Status);
                        break;
                    default:
                        questionsQuery = sortOrder == "desc" ? questionsQuery.OrderByDescending(q => q.CreatedAt) : questionsQuery.OrderBy(q => q.CreatedAt);
                        break;
                }

                var questions = await questionsQuery.ToListAsync();

                ViewBag.PendingCount = questions.Count(q => q.Status != null && q.Status.ToLower() == "pending");
                ViewBag.AnsweredCount = questions.Count(q => q.Status != null && q.Status.ToLower() == "answered");
                ViewBag.TotalCount = questions.Count;

                return View(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải danh sách câu hỏi: {ex.Message}");
                TempData["QuestionsError"] = $"Có lỗi xảy ra khi tải danh sách câu hỏi: {ex.Message}";
                return View(new List<Question>());
            }
        }

        // Trang chi tiết câu hỏi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var question = await _context.Questions
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (question == null) return NotFound();

                // Lấy tất cả câu trả lời cho câu hỏi này
                var answers = await _context.Answers
                    .Where(a => a.QuestionId == id.Value)
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();

                // Lấy danh sách userId -> họ tên (ưu tiên FullName, nếu không có thì Username)
                var userIds = answers.Select(a => a.UserId).Distinct().ToList();
                userIds.Add(question.UserId); // Thêm người hỏi
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                var userDict = users.ToDictionary(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName);

                ViewBag.Answers = answers;
                ViewBag.UserDict = userDict;
                ViewBag.AskerName = userDict.ContainsKey(question.UserId) ? userDict[question.UserId] : $"UserId: {question.UserId}";

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết câu hỏi {Id}", id);
                TempData["QuestionsError"] = "Có lỗi xảy ra khi tải chi tiết câu hỏi.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Xử lý cập nhật trạng thái câu hỏi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null) return NotFound();

                question.Status = status;
                await _context.SaveChangesAsync();

                TempData["QuestionsSuccess"] = $"Đã cập nhật trạng thái câu hỏi thành '{status}'";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái câu hỏi {Id}", id);
                TempData["QuestionsError"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Xử lý thêm câu trả lời cho câu hỏi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswer(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["QuestionsError"] = "Nội dung câu trả lời không được để trống.";
                return RedirectToAction(nameof(Details), new { id = questionId });
            }

            try
            {
                // Lấy UserId từ người đăng nhập
                int userId = 1; // Mặc định admin nếu không đăng nhập
                if (User?.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    if (claim != null && int.TryParse(claim.Value, out int uid))
                        userId = uid;
                }
                var answer = new Answer
                {
                    QuestionId = questionId,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now,
                    UserId = userId
                };

                _context.Answers.Add(answer);
                
                // Cập nhật trạng thái câu hỏi thành "answered"
                var question = await _context.Questions.FindAsync(questionId);
                if (question != null && question.Status == "pending")
                {
                    question.Status = "answered";
                }
                
                await _context.SaveChangesAsync();

                TempData["QuestionsSuccess"] = "Thêm câu trả lời thành công!";
                return RedirectToAction(nameof(Details), new { id = questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm câu trả lời cho câu hỏi {QuestionId}", questionId);
                TempData["QuestionsError"] = "Có lỗi xảy ra khi thêm câu trả lời.";
                return RedirectToAction(nameof(Details), new { id = questionId });
            }
        }

        // Xử lý xóa câu hỏi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);
                
                if (question != null)
                {
                    _context.Questions.Remove(question);
                    await _context.SaveChangesAsync();
                    TempData["QuestionsSuccess"] = "Xóa câu hỏi thành công!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa câu hỏi {Id}", id);
                TempData["QuestionsError"] = "Có lỗi xảy ra khi xóa câu hỏi.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Xử lý xóa câu trả lời
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int answerId, int questionId)
        {
            try
            {
            var answer = await _context.Answers
                .FirstOrDefaultAsync(a => a.Id == answerId);
                
                if (answer != null)
                {
                    _context.Answers.Remove(answer);
                    await _context.SaveChangesAsync();
                    TempData["QuestionsSuccess"] = "Xóa câu trả lời thành công!";
                }
                return RedirectToAction(nameof(Details), new { id = questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa câu trả lời {AnswerId}", answerId);
                TempData["QuestionsError"] = "Có lỗi xảy ra khi xóa câu trả lời.";
                return RedirectToAction(nameof(Details), new { id = questionId });
            }
        }
    }
}
