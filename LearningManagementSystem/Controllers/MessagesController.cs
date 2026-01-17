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


        public async Task<IActionResult> Index(
            string? receiver, 
            bool? isRead,
            int? pageSize,
            int? pageNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            int size = pageSize ?? 5;
            int page = pageNumber ?? 1;

            var result = await _messageService.GetMessagesAsync(
                user.Email,
                receiver, 
                isRead,
                size,
                page);

            ViewBag.CurrentReceiver = receiver;
            ViewBag.CurrentIsRead = isRead; 

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

            IList<ApplicationUser> availableReceivers = new List<ApplicationUser>();

            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                availableReceivers = await _userManager.GetUsersInRoleAsync("Teacher");
            }
            else if (await _userManager.IsInRoleAsync(user, "Teacher")) 
            {
                availableReceivers = await _userManager.GetUsersInRoleAsync("Student");
            }

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
