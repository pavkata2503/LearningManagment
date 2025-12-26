using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    public class CreateQuestionViewModel
    {
        public string Content { get; set; } = string.Empty;
        public AppData.Enums.QuestionType Type { get; set; }
        public List<CreateOptionViewModel> Options { get; set; } = new();
    }
}
