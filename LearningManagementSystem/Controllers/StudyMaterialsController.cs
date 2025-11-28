using AppData.Enums;
using AppData.Models;
using DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.IService;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    //[Authorize]
    public class StudyMaterialsController : Controller
    {
        //[Authorize]
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IStudyMaterialService _service;
        public StudyMaterialsController(IStudyMaterialService service,ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileService fileService)
        {
            _service = service;
            this.context = context;
            this._userManager = userManager;
            this._fileService = fileService;
        }
        

        public async Task<IActionResult> Index(StudyMaterialFilterModel filter)
        {
            var model = await _service.GetFilteredMaterials(filter);

            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(StudyMaterial studyMaterial)
        {

            var userId = User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                throw new ArgumentException("Невалиден потребител.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            studyMaterial.CreatedByName = user.Name;

            if (studyMaterial.FileUpload != null)
            {
                var fileResult = _fileService.SaveImage(studyMaterial.FileUpload);
                if (fileResult.Item1 == 1)
                {
                    studyMaterial.FileTitle = studyMaterial.Title;
                    studyMaterial.FileTitle = fileResult.Item2;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, fileResult.Item2);
                    return View(studyMaterial);
                }
            }
            if (ModelState.IsValid)
            {
                context.StudyMaterials.Add(studyMaterial);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Add", studyMaterial);
        }
        [Authorize(Roles = "Teacher")]
        public IActionResult Edit(int id)
        {
            var studyMaterial = context.StudyMaterials
                .FirstOrDefault(m => m.Id == id);
            if (studyMaterial == null)
            {
                return NotFound();
            }

            return View(studyMaterial);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StudyMaterial studyMaterial)
        {
            var userId = User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                throw new ArgumentException("Invalid user.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            studyMaterial.CreatedByName = user.Name;
            if (studyMaterial.FileUpload != null)
            {
                var fileResult = _fileService.SaveImage(studyMaterial.FileUpload);
                if (fileResult.Item1 == 1)
                {
                    studyMaterial.FileTitle = studyMaterial.Title;
                    studyMaterial.FileTitle = fileResult.Item2;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, fileResult.Item2);
                    return View(studyMaterial);
                }
            }
            else
            {
                var existingMaterial = context.StudyMaterials.AsNoTracking().FirstOrDefault(m => m.Id == studyMaterial.Id);
                if (existingMaterial != null)
                {
                    studyMaterial.FileTitle = existingMaterial.FileTitle;
                }
            }
            if (ModelState.IsValid)
            {

                context.StudyMaterials.Update(studyMaterial);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Edit", studyMaterial);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult Delete(int id)
        {
            var studyMaterial = context.StudyMaterials.Find(id);

            if (studyMaterial == null)
            {
                return NotFound();
            }

            context.StudyMaterials.Remove(studyMaterial);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Details(int id)
        {
            var studyMaterial = context.StudyMaterials.Find(id);

            return View(studyMaterial);
        }
        public IActionResult Ascending()
        {
            var materials = context.StudyMaterials.OrderBy(s => s.CreateDate).ToList();
            return View("Index", materials);
        }

        // Action method to display study materials in descending order of creation date
        public IActionResult Descending()
        {
            var materials = context.StudyMaterials.OrderByDescending(s => s.CreateDate).ToList();
            return View("Index", materials);
        }
    }
}

