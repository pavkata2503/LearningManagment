using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    public class CreateTestViewModel
    {
        public int MaterialId { get; set; }
        public string MaterialTitle { get; set; } = string.Empty;
        public List<CreateQuestionViewModel> Questions { get; set; } = new();
    }
}
