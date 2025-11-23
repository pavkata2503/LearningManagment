using AppData.Enums;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Category = AppData.Enums.Category;

namespace AppData.Models
{
    public class StudyMaterial
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        [Required(ErrorMessage = "Описанието е задължителен")]
        public string Description { get; set; }

        [EnumDataType(typeof(Category))]
        [Required(ErrorMessage = "Категорията е задължителен")]
        public Category Category { get; set; }

        [EnumDataType(typeof(TypeFile))]
        [Required(ErrorMessage = "Вида на файла е задължителен")]
        public TypeFile TypeFile { get; set; }
        [ValidateNever]
        public int AppliationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required(ErrorMessage = "Предметът е задължителен")]
        [StringLength(50, ErrorMessage = "Описанието не трябва да надвишава 50 знака")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Класът е задължителен")]
        [Range(1, 12, ErrorMessage = "Класът е между 1 и 12")]
        public int Class { get; set; }
        public DateTime CreateDate { get; set; }

        [ValidateNever]
        public string CreatedByName { get; set; }
        //Upload File
        [NotMapped]
        public IFormFile? FileUpload { get; set; }
        public string? FileTitle { get; set; }
    }
}
