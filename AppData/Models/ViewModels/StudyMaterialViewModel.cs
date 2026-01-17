using System;
using AppData.Enums; // Увери се, че това е правилното namespace за твоите Enum-и

namespace AppData.Models.ViewModels
{
    public class StudyMaterialViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public TypeFile TypeFile { get; set; }
        public string FileName { get; set; }   
        public string FileUrl { get; set; } 
    }
}