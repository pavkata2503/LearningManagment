using AppData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Add(string? replyTo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // 1. Създаваме празен списък за хората, на които може да се пише
            IList<ApplicationUser> availableReceivers = new List<ApplicationUser>();

            // 2. Проверяваме ролята на текущия потребител
            // ВНИМАНИЕ: Увери се, че имената на ролите ("Student", "Teacher") съвпадат точно с тези в базата ти данни!
            if (await _userManager.IsInRoleAsync(user, "Student")) // Ако е ученик
            {
                // Взимаме всички учители
                availableReceivers = await _userManager.GetUsersInRoleAsync("Teacher");
            }
            else if (await _userManager.IsInRoleAsync(user, "Teacher")) // Ако е учител
            {
                // Взимаме всички ученици
                availableReceivers = await _userManager.GetUsersInRoleAsync("Student");
            }
            else
            {
                // Опционално: Ако е администратор, може би искаш да вижда всички?
                // Засега оставяме списъка празен или може да заредиш всички потребители.
            }

            // 3. Създаваме SelectList за падащото меню
            // Първият параметър е списъкът, вторият е какво да запишем в базата (Email), третият е какво да вижда потребителят (пак Email или Name)
            // Четвъртият параметър (replyTo) избира автоматично правилния човек, ако си натиснал "Отговор"
            //ViewBag.PotentialReceivers = new SelectList(availableReceivers, "Email", "Email", replyTo);
            // "Email" е стойността, която се праща, "Name" (или както е пропъртито за име в ApplicationUser) е това, което се вижда
            ViewBag.PotentialReceivers = new SelectList(availableReceivers, "Email", "Name", replyTo);

            var model = new Message();
            if (!string.IsNullOrEmpty(replyTo))
            {
                model.Receiver = replyTo;
            }

            return View(model);
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
