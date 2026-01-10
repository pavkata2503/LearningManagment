using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    public class QuestionReviewViewModel
    {
        public string Content { get; set; } = string.Empty;
        public List<OptionReviewViewModel> Options { get; set; } = new();
        public int? UserSelectedOptionId { get; set; }
    }
}
