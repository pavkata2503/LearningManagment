using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class UserTestAnswer
    {
        public int Id { get; set; }

        public int UserTestResultId { get; set; }
        public UserTestResult UserTestResult { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public int? SelectedOptionId { get; set; } // Кой отговор е избрал ученикът
        public Option? SelectedOption { get; set; }
    }
}
