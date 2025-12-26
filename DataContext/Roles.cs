using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext
{
        public enum Roles
        {
            [Display(Name = "Админ")]
            Admin,
            [Display(Name = "Учител")]
            Teacher,
            [Display(Name = "Ученик")]
            Student
        }
}
