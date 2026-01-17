using Microsoft.AspNetCore.Mvc.Rendering; // Добави това за SelectListItem
using System.Collections.Generic;

namespace AppData.Models.ViewModels
{
    public class CreateTestViewModel
    {
        public int MaterialId { get; set; }

        public string MaterialTitle { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? MaterialsList { get; set; }

        public List<CreateQuestionViewModel> Questions { get; set; } = new();
    }
}