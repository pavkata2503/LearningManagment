using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models.ViewModels
{
    // За резултата след решаване
    public class TestResultViewModel
    {
        public string MaterialTitle { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double Percentage => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
    }
}
