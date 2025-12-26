using AppData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public QuestionType Type { get; set; }

        public int StudyMaterialId { get; set; } // Свързваме теста с учебен материал или отделен обект Test
        public StudyMaterial? StudyMaterial { get; set; }

        public List<Option> Options { get; set; } = new(); // За избираеми отговори
    }
}
