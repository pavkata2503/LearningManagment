using Microsoft.AspNetCore.Mvc.Rendering; // Добави това за SelectListItem
using System.Collections.Generic;

namespace AppData.Models.ViewModels
{
    public class CreateTestViewModel
    {
        public int MaterialId { get; set; }

        // Това вече не е задължително да е hardcode-нато, защото ще го избираме
        public string MaterialTitle { get; set; } = string.Empty;

        // Новият списък за падащото меню
        public IEnumerable<SelectListItem>? MaterialsList { get; set; }

        public List<CreateQuestionViewModel> Questions { get; set; } = new();
    }
}