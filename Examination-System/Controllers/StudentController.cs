using Examination_System.Models;
using Examination_System.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Examination_System.Controllers
{
    public class StudentController : Controller
    {
        readonly IStudentRepo SRepo; //student repository
        readonly IUserRepo URepo; //user repository

        public StudentController(IStudentRepo _SRepo, IUserRepo _URepo) //constructor
        {
            SRepo = _SRepo;
            URepo = _URepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Exam(int crsId)
        {
            Exam exam = SRepo.GetExamByCrsId(crsId).Result;
            if (exam != null) //check if the exam exists
            {
                return View(exam);
            }
            else
            {
                ViewBag.Message = "No Exam Found";
                return View();
            }
        }

        [HttpPost]
        [ActionName("exam")]
        public IActionResult TakeExam(int examId)
        {
            Exam exam = SRepo.GetExamById(examId).Result;

            int studentId = URepo.GetUserId(User);

            if (SRepo.IsStudentExamSubmitted(examId, studentId).Result) // check if the student submitted the exam
            {
                return RedirectToAction("Result", new { examId, studentId });
            }

            if (exam != null) //check if the exam exists
            {
                return View("TakeExam", exam);
            }
            else
            {
                ViewBag.Message = "No Exam Found";
                return View("TakeExam");
            }
        }

        public IActionResult Result(int examId, int studentId)
        {
            if (SRepo.IsStudentExamSubmitted(examId, studentId).Result) // check if the student submitted the exam
            {
                return View(SRepo.GetStudentCourseDegree(examId, studentId).Result);
            }
            else
            {
                ViewBag.Message = "Exam Not Submitted";
                return View();
            }
        }

        [HttpPost]
        public IActionResult Result(int examId, Dictionary<int, int> studentAnswers)
        {
            List<StudentAnswer> studentAnswersList = new List<StudentAnswer>();
            int studentId = URepo.GetUserId(User);

            foreach (var item in studentAnswers)
            {
                studentAnswersList.Add(new StudentAnswer
                {
                    ExamId = examId,
                    QuestionId = item.Key,
                    StudentId = studentId,
                    SelectedOption = item.Value
                });
            }

            var result = SRepo.SubmitExam(examId, studentId, studentAnswersList).Result;

            if (result) //submit the exam
            {
                StudentCourse studentResult = SRepo.GetStudentCourseDegree(examId, studentId).Result;
                return View(studentResult);
            }
            else
            {
                ViewBag.Message = "Failed to Submit Exam";
                return View();
            }
        }

        public IActionResult Courses()
        {
            int userId = URepo.GetUserId(User);

            return View(SRepo.GetStudentCourses(userId).Result);

        }

        public async Task<IActionResult> Results()
        {
            List<StudentCourse> stdResults = await SRepo.GetStudentResultsByStdId(URepo.GetUserId(User));
            foreach (var item in stdResults)
            {
                Console.WriteLine(item);
            }
            return View(stdResults);
        }
        public async Task<IActionResult> ResultDetails(int id, int crsId)
        {
            try
            {
				Exam exam = await SRepo.GetResultDetailsByStdId(id, crsId);
				ViewBag.answers = SRepo.StudentAnswer(exam.ExamId, id).Result;
				return View(exam);

			}
			catch (Exception e)
            {
                return View("Index");
            }
        }
    }
}
