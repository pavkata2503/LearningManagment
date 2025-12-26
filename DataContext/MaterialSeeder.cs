using AppData.Enums;
using AppData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext
{
    internal class MaterialSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Проверка дали вече има добавени данни
                if (context.StudyMaterials.Any())
                {
                    return;   // Базата данни е вече попълнена с данни
                }

                // Добавяне на примерни учебни материали
                context.StudyMaterials.AddRange(
                    new StudyMaterial
                    {
                        Title = "Дроби",
                        Category = Category.NewMaterial,
                        TypeFile = TypeFile.Table,
                        Subject = "Математика",
                        Class = 10,
                        Description = "Примерно описание на учебен материал 1",
                        CreatedByName = "admin",
                        CreateDate = DateTime.Now
                    },
                    new StudyMaterial
                    {
                        Title = "Атомно ядро",
                        Category = Category.Homework,
                        TypeFile = TypeFile.Table,
                        Subject = "Физика",
                        Class = 11,
                        Description = "Примерно описание на учебен материал 2",
                        CreatedByName = "Pavkata",
                        CreateDate = DateTime.Now
                    },
                    new StudyMaterial
                    {
                        Title = "Първа световна война",
                        Category = Category.NewMaterial,
                        TypeFile = TypeFile.Table,
                        Subject = "История",
                        Class = 10,
                        Description = "Примерно описание на учебен материал 2",
                        CreatedByName = "admin",
                        CreateDate = DateTime.Now
                    }, new StudyMaterial
                    {
                        Title = "Втора световна война",
                        Category = Category.Homework,
                        TypeFile = TypeFile.Table,
                        Subject = "История",
                        Class = 9,
                        Description = "Примерно описание на учебен материал 2",
                        CreatedByName = "admin",
                        CreateDate = DateTime.Now
                    },
                    new StudyMaterial
                    {
                        Title = "Глаголи",
                        Category = Category.ForTesting,
                        TypeFile = TypeFile.Audio,
                        Subject = "Български език",
                        Class = 4,
                        Description = "Примерно описание на учебен материал 2",
                        CreatedByName = "Pavkata",
                        CreateDate = DateTime.Now
                    },
                    new StudyMaterial
                    {
                        Title = "Phersal Verbs",
                        Category = Category.NewMaterial,
                        TypeFile = TypeFile.Diagram,
                        Subject = "Англйски език",
                        Class = 7,
                        Description = "Примерно описание на учебен материал 2",
                        CreatedByName = "admin",
                        CreateDate = DateTime.Now
                    }
                );

                // Записване на промените в базата данни
                context.SaveChanges();
            }
        }
    }
}
