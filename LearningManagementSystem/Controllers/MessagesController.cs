using AppData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(
            IMessageService messageService,
            UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        // MessagesController.cs

        public async Task<IActionResult> Index(
            string? receiver, // Забележка: Тук може би искаш да филтрираш по подател (Sender), а не Receiver, тъй като потребителят вижда своите входящи, но нека го оставим както е по твоята логика
            bool? isRead,
            int? pageSize,
            int? pageNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            int size = pageSize ?? 5;
            int page = pageNumber ?? 1;

            // Взимаме съобщенията (твоята логика си е супер тук)
            var result = await _messageService.GetMessagesAsync(
                user.Email,
                receiver, // Тук се ползва аргумента receiver
                isRead,
                size,
                page);

            // --- НОВО: Запазваме текущите филтри за View-то ---
            ViewBag.CurrentReceiver = receiver;
            ViewBag.CurrentIsRead = isRead; // Това ни трябва за Dropdown-а
                                            // --------------------------------------------------

            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.PageSize = result.PageSize;
            ViewBag.PageSizeOptions = new List<int> { 5, 10, 15, 20 };

            return View(result.Items);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Message message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            ModelState.Remove("Sender");
            ModelState.Remove("SenderEmail");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (!ModelState.IsValid)
                return View(message);

            await _messageService.AddMessageAsync(message, user);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _messageService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _messageService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Този метод отваря съобщението и го прави "Прочетено"
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            await _messageService.MarkAsReadAsync(id);
            var message = await _messageService.GetByIdAsync(id);

            if (message == null) return NotFound();

            return View(message);
        }
    }

}
