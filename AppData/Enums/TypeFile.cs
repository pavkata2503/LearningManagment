using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Enums
{
    public enum TypeFile
    {
        [Display(Name = "Текстов документ")]
        TextDocument,
        [Display(Name = "Снимка")]
        Image,
        [Display(Name = "Таблица")]
        Table,
        [Display(Name = "Диаграма")]
        Diagram,
        [Display(Name = "Презентация")]
        Presentation,
        [Display(Name = "Аудио")]
        Audio,
        [Display(Name = "Видео")]
        Video
    }
}
