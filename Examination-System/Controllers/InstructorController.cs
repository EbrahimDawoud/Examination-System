using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Examination_System.Data;
using Examination_System.Models;
using Examination_System.Repos;

namespace Examination_System.Controllers
{
    public class InstructorController : Controller
    {
        private readonly IInstructorRepo instructorRepo;

        public InstructorController(IInstructorRepo _instructorRepo)
        {
            instructorRepo = _instructorRepo;
        }

        [HttpGet]
        public IActionResult AddQuestion()
        {

            // get the courses for this signed in instructor and send them to the view

            var courses = instructorRepo.GetInstructorCourses(6).Result;
            ViewBag.courses = new SelectList(courses, "CrsId", "CrsName");

            return View(new Question());
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(IFormCollection form, Dictionary<int, string> options = null)
        {
            // get the question data from the form
            var question = new Question
            {
                QuestionText = form["QuestionText"],
                QuestionType = form["QuestionType"],
                CrsId = int.Parse(form["CrsId"]),
                QuestionAnswer = int.Parse(form["QuestionAnswer"])
            };

            // add the question to the database
            var questionId = instructorRepo.AddQuestionToCourse(question).Result;
            if (questionId == -1)
            {
                ModelState.AddModelError("", "Error in adding the question");
                ViewBag.courses = new SelectList(instructorRepo.GetInstructorCourses(6).Result, "CrsId", "CrsName");//change with the signed in instructor id
                return View(question);

            }

            if (question.QuestionType == "MCQ")
            {
                instructorRepo.AddQuestionOption(questionId, options);

            }
            return RedirectToAction("AddQuestion");

        }

        [HttpGet]
        public IActionResult GenerateRandomExam()
        {
            // get the courses for this signed in instructor and send them to the view
            var courses = instructorRepo.GetInstructorCourses(6).Result;
            ViewBag.courses = new SelectList(courses, "CrsId", "CrsName");
            return View(new Exam());
        }

        [HttpPost]
        public async Task<IActionResult> GenerateRandomExam(Exam exam, int MCQCount, int TFCount, int degreeOfMCQ, int degreeOfTF)
        {
            try
            {

                var generatedExamId = await instructorRepo.GenerateRandomExam(exam, MCQCount, TFCount, degreeOfMCQ, degreeOfTF);
                return RedirectToAction("GenerateRandomExam");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                Console.WriteLine(e);
                return View();

                //throw;
            }
        }

    }
}

