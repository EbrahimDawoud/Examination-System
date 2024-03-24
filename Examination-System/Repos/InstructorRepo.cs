﻿using Examination_System.Data;
using Examination_System.Models;
using Examination_System.ModelViews;

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
        public Task<int> GenerateRandomExam(Exam exam, int noOfMCQ, int noOfTF, int degreeOfMCQ, int degreeOfTF);


        // get all instructor courses
        public Task<List<Course>> GetInstructorCourses(int instructorId);


        public Task<List<StudentDegree>> GetStudentsResultByCourse(int crsId);

        // get all students
        public Task<List<User>> GetStudents();

        // get all instructors
        public Task<List<User>> GetInstructors();

        // get all departments
        public Task<List<Department>> GetDepartments();

        // get all courses
        public Task<List<Course>> GetCourses();

        // get all exams
        public Task<List<StudentExam>> GetExams();

        public Task<List<Exam>> Exams();

        public Task<List<ExamQuestion>> ExamQuestions();

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

        public async Task<int> GenerateRandomExam(Exam exam, int noOfMCQ, int noOfTF, int degreeOfMCQ, int degreeOfTF)
        {
            //check if no of mcq and tf are available

            try
            {
                //check if at least one question is available
                if (noOfMCQ == 0 && noOfTF == 0)
                {
                    throw new Exception("At least one question should be added to the exam");
                }


                int noOfMCQInCourse = db.Questions.Count(q => q.CrsId == exam.CrsId && q.QuestionType == "MCQ");
                int noOfTFInCourse = db.Questions.Count(q => q.CrsId == exam.CrsId && q.QuestionType == "TF");

                // throw exception if the no of mcq or tf is less than the required no of mcq or tf with number of questions available
                if (noOfMCQInCourse < noOfMCQ || noOfTFInCourse < noOfTF)
                {
                    throw new Exception($"No enough questions in the course, Available MCQ: {noOfMCQInCourse}, TF: {noOfTFInCourse}");

                }

                // get random mcq questions
                var mcqQuestions = db.Questions.Where(q => q.CrsId == exam.CrsId && q.QuestionType == "MCQ").OrderBy(q => Guid.NewGuid()).Take(noOfMCQ).ToList();

                // get random tf questions
                var tfQuestions = db.Questions.Where(q => q.CrsId == exam.CrsId && q.QuestionType == "TF").OrderBy(q => Guid.NewGuid()).Take(noOfTF).ToList();

                // add the exam to the database
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

        public async Task<List<StudentDegree>> GetStudentsResultByCourse(int crsId)
        {
            try
            {
                
                //get students degrees in this exam
                var studentsDegrees = await db.StudentCourses.Where(se => se.CrsId == crsId).Include(se => se.Student)
                    .Select(se => new StudentDegree
                    {
                        StudentId = se.StudentId,
                        StudentName =
                            db.Users.Where(u => u.UserId == se.StudentId).Select(u => u.UserName).FirstOrDefault(),
                        Degree = se.Grade 
                    }).ToListAsync();

                //check if the students degrees are available


                return  studentsDegrees;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public async Task<List<User>> GetStudents ()
        {
            try
            {
                return await db.Users.Where(u => u.UserRole == "Student").ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }       
        }
        public async Task<List<User>> GetInstructors()
        {
            try
            {
                return await db.Users.Where(u => u.UserRole == "Instructor").ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<Department>> GetDepartments()
        {
            try
            {
                return await db.Departments.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<Course>> GetCourses()
        {
            try
            {
                return await db.Courses.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }   
        public async Task<List<StudentExam>> GetExams()
        {
            try
            {
                return await db.StudentExams.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<Exam>> Exams()
        {
            try
            {
                return await db.Exams.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<ExamQuestion>> ExamQuestions()

        {
            try
            {
                return await db.ExamQuestions.ToListAsync();
            }
                       catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


    }
}
