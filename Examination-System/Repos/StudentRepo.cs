using Examination_System.Data;
using Examination_System.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel;

namespace Examination_System.Repos
{
    public interface IStudentRepo
    {
        public Task<Exam> GetExamByCrsId(int crsId);
        public Task<Exam> GetExamById(int examId);
        public Task<bool> SubmitExam(int examId, int studentId, List<StudentAnswer> studentAnswers);
        public Task<StudentExam> GetStudentExamDegree(int examId, int studentId);
        public Task<bool> IsStudentExamSubmitted(int examId, int studentId);
        public Task<List<StudentCourse>> GetStudentCourses(int id);
        public Task<List<StudentCourse>> GetStudentResultsByStdId(int stdId);
        public Task<Exam> GetResultDetailsByStdId(int stdId , int crsId);
        public Task<List<StudentAnswer>> StudentAnswer(int examId, int studentId);

    }

    public class StudentRepo : IStudentRepo
    {
        readonly ExaminationSystemContext db;

        public StudentRepo(ExaminationSystemContext _db)
        {
            db = _db;
        }

        public async Task<Exam> GetExamByCrsId(int crsId) //get exam by course id
        {
            try
            {
                return await db.Exams.Where(e => e.CrsId == crsId).Include(c => c.Crs).Include(e => e.ExamQuestions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Exam> GetExamById(int examId) //get exam by id
        {
            try
            {
                return await db.Exams.Where(e => e.ExamId == examId).Include(c => c.Crs).Include(e => e.ExamQuestions).ThenInclude(q => q.Question).ThenInclude(qo => qo.QuestionOptions).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> SubmitExam(int examId, int studentId , List<StudentAnswer> studentAnswers) //submit the exam
        {
            int grade = 0;
            //float finalGrade = db.ExamQuestions.Where(eq => eq.ExamId == studentAnswers[0].ExamId).Sum(eq => eq.Degree); //get the total degree of the exam
            
            try
            {
                foreach (var answer in studentAnswers)
                {
                    var questionGrade = await db.ExamQuestions.Where(eq => eq.ExamId == answer.ExamId && eq.QuestionId == answer.QuestionId).Select(eq => eq.Degree).FirstOrDefaultAsync(); //get the degree of the question
                    if(answer.SelectedOption == 1)
                    {
                        grade += questionGrade; //add the degree of the question to the total grade
                    }
                    db.StudentAnswers.Add(answer);
                }

                await db.SaveChangesAsync();


                StudentExam studentExam = new StudentExam
                {
                    ExamId = examId,
                    StdId = studentId,
                    ExamDate = DateOnly.FromDateTime(DateTime.Now),
                    Grade = grade
                };

                db.StudentExams.Add(studentExam);

                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        
        public async Task<StudentExam> GetStudentExamDegree(int examId, int studentId) //get the student exam degree
        {
            try
            {
                return await db.StudentExams.Where(se => se.ExamId == examId && se.StdId == studentId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> IsStudentExamSubmitted(int examId, int studentId) //check if the student exam is submitted
        {
            try
            {
                return await db.StudentExams.AnyAsync(se => se.ExamId == examId && se.StdId == studentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<StudentCourse>> GetStudentCourses(int id) //get the student courses
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@StdId", id)
                };

                return await db.StudentCourses.FromSqlRaw("EXECUTE GetStudentCourses @StdId", parameters).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<StudentCourse>> GetStudentResultsByStdId(int stdId)
        {
            try
            {
                return await db.StudentCourses.Where(sc => sc.StudentId == stdId).Include(sc => sc.Crs).Where(s => s.Grade != null).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
		public async Task<Exam> GetResultDetailsByStdId(int stdId , int crsId)
		{
			var StdExam = await db.StudentExams.FirstOrDefaultAsync(se => se.StdId == stdId);
			if (StdExam != null)
			{
				int examId = StdExam.ExamId;
                if(db.Exams.FirstOrDefault(s=>s.ExamId == examId).CrsId == crsId)
                {
					return await db.Exams.Where(e => e.ExamId == examId).Include(c => c.Crs).Include(e => e.ExamQuestions).ThenInclude(q => q.Question).ThenInclude(qo => qo.QuestionOptions).FirstOrDefaultAsync();
				}
                else { return null; }
			}
			else
			{
				return null;
			}
		}
		public async Task<List<StudentAnswer>> StudentAnswer(int examId, int studentId)
		{
			try
			{
				return await db.StudentAnswers.Where(sa => sa.ExamId == examId && sa.StudentId == studentId).ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
    }
}
