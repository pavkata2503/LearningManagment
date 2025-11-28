using AppData.Enums;
using AppData.Models;
using DataContext;
using Services.IService;
using Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class StudyMaterialService:IStudyMaterialService
    {
        private readonly ApplicationDbContext _context;

        public StudyMaterialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<StudyMaterial>> GetFilteredMaterials(StudyMaterialFilterModel filter)
        {
            var materials = _context.StudyMaterials.AsQueryable();

            // Filter: Created by name
            if (!string.IsNullOrEmpty(filter.CreatedName))
                materials = materials.Where(m => m.CreatedByName == filter.CreatedName);

            // Search
            if (!string.IsNullOrEmpty(filter.Search))
                materials = materials.Where(m =>
                    m.Title.Contains(filter.Search) ||
                    m.Description.Contains(filter.Search));

            // Category filter
            if (!string.IsNullOrEmpty(filter.Category) &&
                Enum.TryParse<Category>(filter.Category, out var parsedCategory))
                materials = materials.Where(m => m.Category == parsedCategory);

            // TypeFile filter
            if (!string.IsNullOrEmpty(filter.TypeFile) &&
                Enum.TryParse<TypeFile>(filter.TypeFile, out var parsedTypeFile))
                materials = materials.Where(m => m.TypeFile == parsedTypeFile);

            // Subject
            if (!string.IsNullOrEmpty(filter.Subject))
                materials = materials.Where(m => m.Subject == filter.Subject);

            // Class
            if (!string.IsNullOrEmpty(filter.ClassFilter))
                materials = materials.Where(m => (int)m.Class == int.Parse(filter.ClassFilter));

            // Sorting
            materials = filter.SortOrder switch
            {
                "date_desc" => materials.OrderByDescending(m => m.CreateDate),
                "date_asc" => materials.OrderBy(m => m.CreateDate),
                _ => materials.OrderBy(m => m.CreateDate),
            };

            return await PaginatedList<StudyMaterial>
                .CreateAsync(materials, filter.PageNumber, filter.PageSize);
        }
    }
}
