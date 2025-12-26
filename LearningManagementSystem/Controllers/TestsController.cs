using AppData.Enums;
using AppData.Models;
using AppData.Models.ViewModels;
using DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Показване на теста
        public async Task<IActionResult> Solve(int materialId)
        {
            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.StudyMaterialId == materialId)
                .ToListAsync();

            var model = new SolveTestViewModel
            {
                MaterialId = materialId,
                Questions = questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    Options = q.Options.Select(o => new OptionViewModel { Id = o.Id, Text = o.Text }).ToList()
                }).ToList()
            };

            return View(model);
        }

        // Изпращане на отговорите
        [HttpPost]
        public async Task<IActionResult> Submit(SolveTestViewModel model)
        {
            int score = 0;
            foreach (var q in model.Questions)
            {
                var dbQuestion = await _context.Questions
                    .Include(x => x.Options)
                    .FirstAsync(x => x.Id == q.QuestionId);

                if (dbQuestion.Type == QuestionType.MultipleChoice)
                {
                    if (dbQuestion.Options.Any(o => o.Id == q.SelectedOptionId && o.IsCorrect))
                        score++;
                }
                else // Отворен отговор - тук логиката може да е по-сложна (ръчна проверка или точен текст)
                {
                    // Пример: Проверка за точно съвпадение (не е препоръчително за сложни изречения)
                    // Обикновено отворените отговори се проверяват от учител.
                }
            }

            var result = new UserTestResult
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                StudyMaterialId = model.MaterialId,
                Score = score,
                CompletedOn = DateTime.Now
            };

            _context.UserTestResults.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Results", new { id = result.Id });
        }

        // GET: Отваряне на формата за създаване
        public async Task<IActionResult> Create(int materialId)
        {
            var material = await _context.StudyMaterials.FindAsync(materialId);
            if (material == null) return NotFound();

            var model = new CreateTestViewModel
            {
                MaterialId = materialId,
                MaterialTitle = material.Title
            };
            return View(model);
        }

        // POST: Записване на теста
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var qModel in model.Questions)
                {
                    var question = new Question
                    {
                        Content = qModel.Content,
                        Type = qModel.Type,
                        StudyMaterialId = model.MaterialId
                    };

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync(); // Записваме, за да получим ID на въпроса

                    if (qModel.Type == QuestionType.MultipleChoice)
                    {
                        foreach (var optModel in qModel.Options)
                        {
                            var option = new Option
                            {
                                Text = optModel.Text,
                                IsCorrect = optModel.IsCorrect,
                                QuestionId = question.Id
                            };
                            _context.Options.Add(option);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "StudyMaterials");
            }
            return View(model);
        }
    }
}

