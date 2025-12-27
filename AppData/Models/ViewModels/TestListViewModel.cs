using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    // За списъка с всички тестове
    public class TestListViewModel
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuestionsCount { get; set; }
    }
}
