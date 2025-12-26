using AppData.Models;
using AppData.Models.ViewModels;
using Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IStudyMaterialService
    {
        Task<PaginatedList<StudyMaterial>> GetFilteredMaterials(StudyMaterialFilterModel filter);
        Task<(bool IsSuccess, string ErrorMessage)> AddAsync(
        StudyMaterial studyMaterial,
        ClaimsPrincipal user);
    }

}
