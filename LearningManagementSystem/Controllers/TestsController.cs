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
                    await _context.SaveChangesAsync(); 

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

        public async Task<IActionResult> Index()
        {
            var tests = await _context.StudyMaterials
                .Where(m => _context.Questions.Any(q => q.StudyMaterialId == m.Id)) 
                .Select(m => new TestListViewModel
                {
                    MaterialId = m.Id,
                    Title = m.Title,
                    Category = m.Category.ToString(),
                    QuestionsCount = _context.Questions.Count(q => q.StudyMaterialId == m.Id)
                }).ToListAsync();

            return View(tests);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solve(SolveTestViewModel model)
        {
            int correctCount = 0;
            
            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.StudyMaterialId == model.MaterialId)
                .ToListAsync();

            var userAnswersList = new List<UserTestAnswer>();

            foreach (var submittedQ in model.Questions)
            {
                var dbQ = questions.FirstOrDefault(q => q.Id == submittedQ.QuestionId);
                if (dbQ == null) continue;

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

            var result = new UserTestResult
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                StudyMaterialId = model.MaterialId,
                Score = correctCount,
                CompletedOn = DateTime.Now,
                Answers = userAnswersList 
            };

            _context.UserTestResults.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { id = result.Id });
        }

        public async Task<IActionResult> Result(int id)
        {
            var result = await _context.UserTestResults
                .Include(r => r.StudyMaterial)
                .Include(r => r.User) 
                .Include(r => r.Answers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isTeacher = User.IsInRole("Teacher");

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
                StudentUsername = result.User?.UserName ?? "Неизвестен", 
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
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int materialId)
        {
            var questions = await _context.Questions
                .Where(q => q.StudyMaterialId == materialId)
                .ToListAsync();

            if (!questions.Any())
            {
                return NotFound();
            }

            _context.Questions.RemoveRange(questions);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

