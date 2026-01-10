using AppData.Enums;
using AppData.Models;
using AppData.Models.ViewModels;
using DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        //// Показване на теста
        //public async Task<IActionResult> Solve(int materialId)
        //{
        //    var questions = await _context.Questions
        //        .Include(q => q.Options)
        //        .Where(q => q.StudyMaterialId == materialId)
        //        .ToListAsync();

        //    var model = new SolveTestViewModel
        //    {
        //        MaterialId = materialId,
        //        Questions = questions.Select(q => new QuestionViewModel
        //        {
        //            QuestionId = q.Id,
        //            Content = q.Content,
        //            Type = q.Type,
        //            Options = q.Options.Select(o => new OptionViewModel { Id = o.Id, Text = o.Text }).ToList()
        //        }).ToList()
        //    };

        //    return View(model);
        //}

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

        [HttpGet]
        public async Task<IActionResult> Create(int? materialId)
        {
            // Взимаме всички материали и маркираме избрания, ако има подадено ID
            var materials = await _context.StudyMaterials
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Title,
                    Selected = materialId.HasValue && m.Id == materialId
                })
                .ToListAsync();

            var model = new CreateTestViewModel
            {
                MaterialsList = materials,
                MaterialId = materialId ?? 0
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

        // 1. Списък на всички налични тестове
        public async Task<IActionResult> Index()
        {
            var tests = await _context.StudyMaterials
                .Where(m => _context.Questions.Any(q => q.StudyMaterialId == m.Id)) // Вземи само материали, които имат въпроси
                .Select(m => new TestListViewModel
                {
                    MaterialId = m.Id,
                    Title = m.Title,
                    Category = m.Category.ToString(),
                    QuestionsCount = _context.Questions.Count(q => q.StudyMaterialId == m.Id)
                }).ToListAsync();

            return View(tests);
        }

        // 2. Страница за решаване (GET)
        public async Task<IActionResult> Solve(int id)
        {
            var material = await _context.StudyMaterials
                .Include(m => m.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            var model = new SolveTestViewModel
            {
                MaterialId = material.Id,
                Title = material.Title,
                Questions = material.Questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };

            return View(model);
        }

        // 3. Обработка на отговорите (POST) - ПРОМЕНЕН
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solve(SolveTestViewModel model)
        {
            int correctCount = 0;
            
            // Взимаме въпросите от базата, за да сме сигурни, че проверяваме правилно
            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.StudyMaterialId == model.MaterialId)
                .ToListAsync();

            var userAnswersList = new List<UserTestAnswer>();

            foreach (var submittedQ in model.Questions)
            {
                var dbQ = questions.FirstOrDefault(q => q.Id == submittedQ.QuestionId);
                if (dbQ == null) continue;

                // Създаваме запис за отговора на потребителя
                var userAnswer = new UserTestAnswer
                {
                    QuestionId = dbQ.Id,
                    SelectedOptionId = submittedQ.SelectedOptionId
                };
                userAnswersList.Add(userAnswer);

                if (dbQ.Type == QuestionType.MultipleChoice)
                {
                    if (dbQ.Options.Any(o => o.Id == submittedQ.SelectedOptionId && o.IsCorrect))
                    {
                        correctCount++;
                    }
                }
            }

            // Записваме резултата + детайлните отговори
            var result = new UserTestResult
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                StudyMaterialId = model.MaterialId,
                Score = correctCount,
                CompletedOn = DateTime.Now,
                Answers = userAnswersList // Entity Framework автоматично ще ги свърже
            };

            _context.UserTestResults.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { id = result.Id });
        }

        // 4. Показване на крайния резултат (За ученика ИЛИ за учителя)
        public async Task<IActionResult> Result(int id)
        {
            var result = await _context.UserTestResults
                .Include(r => r.StudyMaterial)
                .Include(r => r.User) // ВАЖНО: Взимаме и потребителя (ученика)
                .Include(r => r.Answers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound();

            // СИГУРНОСТ: Проверка дали потребителят има право да вижда този резултат
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isTeacher = User.IsInRole("Teacher");

            // Ако не си учителят и не си авторът на теста -> Забранено
            if (result.UserId != currentUserId && !isTeacher)
            {
                return Forbid();
            }

            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.StudyMaterialId == result.StudyMaterialId)
                .ToListAsync();

            var model = new TestResultViewModel
            {
                MaterialTitle = result.StudyMaterial.Title,
                StudentUsername = result.User?.UserName ?? "Неизвестен", // Попълваме името
                TotalQuestions = questions.Count,
                CorrectAnswers = result.Score,
                QuestionsReview = questions.Select(q => new QuestionReviewViewModel
                {
                    Content = q.Content,
                    UserSelectedOptionId = result.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.SelectedOptionId,
                    Options = q.Options.Select(o => new OptionReviewViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return View(model);
        }

        // 5. НОВО: Статистика за учители
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AllStudentResults()
        {
            var results = await _context.UserTestResults
                .Include(r => r.User)
                .Include(r => r.StudyMaterial)
                .OrderByDescending(r => r.CompletedOn)
                .ToListAsync();

            return View(results);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")] // Само учители могат да трият
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int materialId)
        {
            // 1. Намираме всички въпроси за този материал
            var questions = await _context.Questions
                .Where(q => q.StudyMaterialId == materialId)
                .ToListAsync();

            if (!questions.Any())
            {
                return NotFound();
            }

            // 2. Изтриваме ги наведнъж
            _context.Questions.RemoveRange(questions);

            // 3. Запазваме промените
            await _context.SaveChangesAsync();

            // Връщаме се към списъка с тестове
            return RedirectToAction(nameof(Index));
        }
    }
}

