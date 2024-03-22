using Examination_System.Data;
using Examination_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Examination_System.Repos
{
    public interface IInstructorRepo
    {
        // add question and return id of the question 
        public Task<int> AddQuestionToCourse(Question question);
        // add options to the question

        public Task<bool> AddQuestionOption(int questionId, Dictionary<int, string> options);

        // generate random exam 
        public Task<int> GenerateRandomExam(int crsId, int examDuration, DateOnly examDate,
            int noOfMCQ, int noOfTF, int degreeOfMCQ, int degreeOfTF);

        // get all instructor courses
        public Task<List<Course>> GetInstructorCourses(int instructorId);



    }
    public class InstructorRepo : IInstructorRepo
    {
        readonly ExaminationSystemContext db;

        public InstructorRepo(ExaminationSystemContext _db)
        {
            db = _db;
        }


        public async Task<List<Course>> GetInstructorCourses(int instructorId)
        {
            try
            {
                return await db.Crsdepins.Where(cds => cds.InstructorId == instructorId).Include(cds => cds.Crs).Select(cds => cds.Crs).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> AddQuestionToCourse(Question question)
        {
            try
            {
                db.Questions.Add(question);
                await db.SaveChangesAsync();
                return question.QuestionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public async Task<bool> AddQuestionOption(int questionId, Dictionary<int, string> options)
        {
            try
            {
                foreach (var option in options)
                {
                    QuestionOption questionOption = new QuestionOption
                    {
                        QuestionId = questionId,
                        OptionNo = option.Key,
                        OptionText = option.Value
                    };

                    db.QuestionOptions.Add(questionOption);
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //delete the question if the options are not added
                db.Questions.Remove(db.Questions.Find(questionId));
                return false;
            }
        }

        public async Task<int> GenerateRandomExam(int crsId, int examDuration, DateOnly examDate, int noOfMCQ, int noOfTF, int degreeOfMCQ, int degreeOfTF)
        {
            //check if no of mcq and tf are available

            try
            {
                int noOfMCQInCourse = db.Questions.Count(q => q.CrsId == crsId && q.QuestionType == "MCQ");
                int noOfTFInCourse = db.Questions.Count(q => q.CrsId == crsId && q.QuestionType == "TF");

                // throw exception if the no of mcq or tf is less than the required no of mcq or tf with number of questions available
                if (noOfMCQInCourse < noOfMCQ || noOfTFInCourse < noOfTF)
                {
                    throw new Exception("No enough questions in the course");
                }

                // get random mcq questions
                var mcqQuestions = db.Questions.Where(q => q.CrsId == crsId && q.QuestionType == "MCQ").OrderBy(q => Guid.NewGuid()).Take(noOfMCQ).ToList();

                // get random tf questions
                var tfQuestions = db.Questions.Where(q => q.CrsId == crsId && q.QuestionType == "TF").OrderBy(q => Guid.NewGuid()).Take(noOfTF).ToList();

                // add the exam to the database
                Exam exam = new Exam
                {
                    CrsId = crsId,
                    Duration = examDuration,
                    GenerationDate = examDate
                };

                db.Exams.Add(exam);

                 db.SaveChanges();
                //check if the exam is added
                if (exam.ExamId == 0)
                {
                    throw new Exception("Error in adding the exam");
                }

                // add the questions to the exam
                foreach (var question in mcqQuestions)
                {
                    ExamQuestion examQuestion = new ExamQuestion
                    {
                        ExamId = exam.ExamId,
                        QuestionId = question.QuestionId,
                        Degree = degreeOfMCQ
                    };

                    db.ExamQuestions.Add(examQuestion);
                }

                db.SaveChanges();

                foreach (var question in tfQuestions)
                {
                    ExamQuestion examQuestion = new ExamQuestion
                    {
                        ExamId = exam.ExamId,
                        QuestionId = question.QuestionId,
                        Degree = degreeOfTF

                    };

                    db.ExamQuestions.Add(examQuestion);
                }
                db.SaveChanges();

                return exam.ExamId;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }



        }
    }
}
