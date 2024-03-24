﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Examination_System.Data;
using Examination_System.Models;
using Examination_System.Repos;
using Microsoft.AspNetCore.Authorization;

namespace Examination_System.Controllers
{
    [Authorize (Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IInstructorRepo instructorRepo;
        private readonly IUserRepo userRepo;

        public InstructorController(IInstructorRepo _instructorRepo, IUserRepo _userRepo)
        {
            instructorRepo = _instructorRepo;
            userRepo = _userRepo;
        }

        [HttpGet]
        public IActionResult AddQuestion()
        {

            // get the courses for this signed in instructor and send them to the view

            var courses = instructorRepo.GetInstructorCourses(userRepo.GetUserId(User)).Result;
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
                ViewBag.courses = new SelectList(instructorRepo.GetInstructorCourses(userRepo.GetUserId(User)).Result, "CrsId", "CrsName");//change with the signed in instructor id
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
            var courses = instructorRepo.GetInstructorCourses(userRepo.GetUserId(User)).Result;
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

        [HttpGet]
        public async Task<IActionResult> ShowResults()
        {

            var courses = await instructorRepo.GetInstructorCourses(userRepo.GetUserId(User));

            ViewBag.courses = new SelectList(courses, "CrsId", "CrsName");

            return View();
        }

        public async Task<IActionResult> GetStudentsResultByCourse(int crsId)
        {

            var students = await instructorRepo.GetStudentsResultByCourse(crsId);
            return PartialView(students);
        }

        public async Task<IActionResult> GenerateReports()
        {
            var students = await instructorRepo.GetStudents();
            var Instructors = await instructorRepo.GetInstructors();
            var Departments = await instructorRepo.GetDepartments();
            var Courses = await instructorRepo.GetCourses();
            var studentExams = await instructorRepo.GetExams();
            var Exams = await instructorRepo.Exams();
            var ExamQuestions = await instructorRepo.ExamQuestions();
            ViewBag.students = students;
            ViewBag.instructors = Instructors;
            ViewBag.departments = Departments;
            ViewBag.courses = Courses;
            ViewBag.studentExams = studentExams;
            ViewBag.exams = Exams;
            ViewBag.examQuestions = ExamQuestions;
            return View();
        }

    }
}

