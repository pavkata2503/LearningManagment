using AppData.Enums;
using AppData.Models;
using AppData.Models.ViewModels;
using DataContext;
using Microsoft.AspNetCore.Identity;
using Services.IService;
using Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class StudyMaterialService:IStudyMaterialService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public StudyMaterialService(
       ApplicationDbContext context,
       UserManager<ApplicationUser> userManager,
       IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<PaginatedList<StudyMaterial>> GetFilteredMaterials(StudyMaterialFilterModel filter)
        {
            var materials = _context.StudyMaterials.AsQueryable();

            if (!string.IsNullOrEmpty(filter.CreatedName))
                materials = materials.Where(m => m.CreatedByName == filter.CreatedName);

            if (!string.IsNullOrEmpty(filter.Search))
                materials = materials.Where(m =>
                    m.Title.Contains(filter.Search) ||
                    m.Description.Contains(filter.Search));

            if (!string.IsNullOrEmpty(filter.Category) &&
                Enum.TryParse<Category>(filter.Category, out var parsedCategory))
                materials = materials.Where(m => m.Category == parsedCategory);

            if (!string.IsNullOrEmpty(filter.TypeFile) &&
                Enum.TryParse<TypeFile>(filter.TypeFile, out var parsedTypeFile))
                materials = materials.Where(m => m.TypeFile == parsedTypeFile);

            if (!string.IsNullOrEmpty(filter.Subject))
                materials = materials.Where(m => m.Subject == filter.Subject);

            //if (!string.IsNullOrEmpty(filter.ClassFilter))
            //    materials = materials.Where(m => (int)m.Class == int.Parse(filter.ClassFilter));

            materials = filter.SortOrder switch
            {
                "date_desc" => materials.OrderByDescending(m => m.CreateDate),
                "date_asc" => materials.OrderBy(m => m.CreateDate),
                _ => materials.OrderBy(m => m.CreateDate),
            };

            return await PaginatedList<StudyMaterial>
                .CreateAsync(materials, filter.PageNumber, filter.PageSize);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddAsync(
       StudyMaterial studyMaterial,
       ClaimsPrincipal userClaims)
        {
            var userId = userClaims
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;

            if (userId == null)
            {
                return (false, "Невалиден потребител.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            studyMaterial.CreatedByName = user.Email;

            if (studyMaterial.FileUpload != null)
            {
                var fileResult = _fileService.SaveImage(studyMaterial.FileUpload);

                if (fileResult.Item1 != 1)
                {
                    return (false, fileResult.Item2);
                }

                studyMaterial.FileTitle = fileResult.Item2;
            }

            _context.StudyMaterials.Add(studyMaterial);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        

        public async Task MarkAsReadAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}

