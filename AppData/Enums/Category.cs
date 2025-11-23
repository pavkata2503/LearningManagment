using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Enums
{
    public enum Category
    {
        [Display(Name = "Нов Материал")]
        NewMaterial,
		[Display(Name = "Упражнение")]
        Exercise,
		[Display(Name = "Домашна работа")]
        Homework,
		[Display(Name = "За тест")]
        ForTesting
    }
}
