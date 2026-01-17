using AppData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public QuestionType Type { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();

        public int? SelectedOptionId { get; set; } 
        public string? OpenAnswer { get; set; }   
    }
}
