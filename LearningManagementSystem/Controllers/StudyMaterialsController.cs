using AppData.Enums;
using AppData.Models;
using AppData.Models.ViewModels;
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


        //public async Task<IActionResult> Index(StudyMaterialFilterModel filter)
        //{
        //    var model = await _service.GetFilteredMaterials(filter);

        //    return View(model);
        //}
        [HttpGet]
        public async Task<IActionResult> Index(StudyMaterialFilterModel filterModel, int? pageNumber)
        {
            // 1. Настройваме страниците във филтър модела, преди да го подадем на сервиза
            filterModel.PageNumber = pageNumber ?? 1;
            filterModel.PageSize = 6; // Тук задаваш по колко елемента на страница искаш

            // 2. Извикваме правилния метод от твоя сървис
            var materials = await _service.GetFilteredMaterials(filterModel);

            // 3. Прехвърляме данните към ViewModel-а за изгледа
            // Внимавай с имената на свойствата тук (напаснал съм ги спрямо твоя сървис)
            var viewModelItems = materials.Select(m => new StudyMaterialViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Category = m.Category,
                CreatedOn = m.CreateDate,   // В твоя модел е CreateDate
                TypeFile = m.TypeFile,
                FileName = m.FileTitle,     // В твоя модел е FileTitle
                FileUrl = $"/Uploads/{m.FileTitle}"
            }).ToList();

            // 4. Създаваме резултата
            var result = new PaginatedResult<StudyMaterialViewModel>
            {
                Items = viewModelItems,
                CurrentPage = materials.PageIndex,
                TotalPages = materials.TotalPages
            };

            return View(result);
        }



        //[Authorize(Roles = "Teacher")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(StudyMaterial studyMaterial)
        {
            if (!ModelState.IsValid)
            {
                return View(studyMaterial);
            }

            var result = await _service.AddAsync(studyMaterial, User);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(studyMaterial);
            }

            return RedirectToAction(nameof(Index));
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
            studyMaterial.CreatedByName = user.Email;
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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Използваме async/await за по-добра производителност
            var material = await context.StudyMaterials
                .AsNoTracking() // Оптимизация за read-only
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            // Мапваме към ViewModel (същата логика като в Index)
            var model = new StudyMaterialViewModel
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                Category = material.Category,
                CreatedOn = material.CreateDate,
                TypeFile = material.TypeFile,
                FileName = material.FileTitle,
                // Тук конструираме пътя към файла, както в Index метода
                FileUrl = $"/Uploads/{material.FileTitle}"
            };

            return View(model);
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

