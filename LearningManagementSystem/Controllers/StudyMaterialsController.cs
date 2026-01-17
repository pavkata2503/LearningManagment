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
    public class StudyMaterialsController : Controller
    {
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

        [HttpGet]
        public async Task<IActionResult> Index(StudyMaterialFilterModel filterModel, int? pageNumber)
        {
            filterModel.PageNumber = pageNumber ?? 1;
            filterModel.PageSize = 6; 

            var materials = await _service.GetFilteredMaterials(filterModel);

            var viewModelItems = materials.Select(m => new StudyMaterialViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Category = m.Category,
                CreatedOn = m.CreateDate,   
                TypeFile = m.TypeFile,
                FileName = m.FileTitle,    
                FileUrl = $"/Uploads/{m.FileTitle}"
            }).ToList();

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
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var studyMaterial = await context.StudyMaterials.FindAsync(id);

            if (studyMaterial == null)
            {
                return NotFound();
            }

            return View(studyMaterial);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(StudyMaterial model)
        {
            var existingMaterial = await context.StudyMaterials.FindAsync(model.Id);

            if (existingMaterial == null)
            {
                return NotFound();
            }

            if (model.FileUpload != null)
            {
                var fileResult = _fileService.SaveImage(model.FileUpload);

                if (fileResult.Item1 == 1) 
                {
                    existingMaterial.FileTitle = fileResult.Item2; 
                }
                else 
                {
                    ModelState.AddModelError(string.Empty, fileResult.Item2);
                    return View(model);
                }
            }
            existingMaterial.Title = model.Title;
            existingMaterial.Description = model.Description;
            existingMaterial.Category = model.Category;
            existingMaterial.TypeFile = model.TypeFile;
            existingMaterial.Subject = model.Subject;
            //existingMaterial.Class = model.Class;
            existingMaterial.URL = model.URL; 

            if (ModelState.IsValid)
            {
                
                await context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            
            return View(model);
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
           
            var material = await context.StudyMaterials
                .AsNoTracking() // Оптимизация за read-only
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

           
            var model = new StudyMaterialViewModel
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                Category = material.Category,
                CreatedOn = material.CreateDate,
                TypeFile = material.TypeFile,
                FileName = material.FileTitle,
                FileUrl = $"/Uploads/{material.FileTitle}"
            };

            return View(model);
        }
        public IActionResult Ascending()
        {
            var materials = context.StudyMaterials.OrderBy(s => s.CreateDate).ToList();
            return View("Index", materials);
        }
        public IActionResult Descending()
        {
            var materials = context.StudyMaterials.OrderByDescending(s => s.CreateDate).ToList();
            return View("Index", materials);
        }
    }
}

