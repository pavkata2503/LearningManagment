using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Class { get; set; }
        public List<Message> Messages { get; set; }
        public List<StudyMaterial> StudyMaterials { get; set; }
    }
}
