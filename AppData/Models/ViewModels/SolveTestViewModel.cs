using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    public class SolveTestViewModel
    {
        public int MaterialId { get; set; }
        public string Title { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
}
