using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class UserTestResult
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int StudyMaterialId { get; set; }
        public int Score { get; set; } 
        public DateTime CompletedOn { get; set; } = DateTime.Now;
        public StudyMaterial StudyMaterial { get; set; } = null!;
        public List<UserTestAnswer> Answers { get; set; } = new();
    }
}
