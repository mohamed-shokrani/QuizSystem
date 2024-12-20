﻿using Core.Models;
using Infrastructure.Data;
using Infrastructure.GenericRepository;
using Infrastructure.Services;
using Infrastructure.Services.CourseService;
using Infrastructure.Services.ExamService;
using Infrastructure.Services.QuestionService;

namespace Infrastructure.UnitOfWork;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IGenericRepository<Instructor> Instructors { get; private set; }
    public IGenericRepository<Student> Students { get; private set; }

    public IGenericRepository<Course> Courses { get; private set; }

    public IGenericRepository<Exam> Exams { get; private set; }

    public IGenericRepository<Question> Questions { get; private set; }

    public IGenericRepository<Answer> Answers { get; private set; }

    public IGenericRepository<CourseStudent> CourseStudents { get; private set; }
    public IGenericRepository<CourseInstructor> CourseInstructors { get; private set; }

    public IGenericRepository<Choice> Choices { get; private set; }

    public IGenericRepository<ExamStudent> ExamStudents { get; private set; }
    public IExamService<Exam> ExamService { get; private set; }
    public IQuestionService<Question> QuestionService { get; private set; }

    public ICourseServicee<Course> CourseService { get; private set; }


    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Instructors = new GenericRepository<Instructor>(_context);
        Students = new GenericRepository<Student>(_context);
        Exams = new GenericRepository<Exam>(_context);
        Questions = new GenericRepository<Question>(_context);
        Answers = new GenericRepository<Answer>(_context);
        Courses = new GenericRepository<Course>(_context);
        CourseStudents = new GenericRepository<CourseStudent>(_context);
        CourseInstructors = new GenericRepository<CourseInstructor>(_context);
        Choices = new GenericRepository<Choice>(_context);
        ExamStudents = new GenericRepository<ExamStudent>(_context);
        ExamService = new ExamService<Exam>(this);
        CourseService = new CourseService<Course>(this);
        QuestionService = new QuestionService<Question>(this);


    }
    public async Task<int> Complete()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
