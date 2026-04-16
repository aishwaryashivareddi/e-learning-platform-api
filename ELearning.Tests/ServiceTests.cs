using AutoMapper;
using ELearning.API.Mappings;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;
using ELearning.Infrastructure.Repositories;
using ELearning.Infrastructure.Services;

namespace ELearning.Tests;

public class CourseServiceTests
{
    private readonly IMapper _mapper;

    public CourseServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
    }

    [Fact]
    public async Task CreateCourse_ReturnsDto()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(CreateCourse_ReturnsDto));
        ctx.Users.Add(new User { UserId = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "x" });
        await ctx.SaveChangesAsync();

        var svc = new CourseService(new Repository<Course>(ctx), ctx, _mapper);
        var result = await svc.CreateAsync(new CourseCreateDto("C# Basics", "Learn C#", 1));

        Assert.Equal("C# Basics", result.Title);
        Assert.True(result.CourseId > 0);
    }

    [Fact]
    public async Task GetById_WithLessonsAndQuizzes_ReturnsDetail()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(GetById_WithLessonsAndQuizzes_ReturnsDetail));
        ctx.Users.Add(new User { UserId = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "x" });
        var course = new Course { Title = "EF Core", Description = "ORM", CreatedBy = 1 };
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        ctx.Lessons.Add(new Lesson { CourseId = course.CourseId, Title = "Intro", Content = "...", OrderIndex = 1 });
        ctx.Quizzes.Add(new Quiz { CourseId = course.CourseId, Title = "Quiz 1" });
        await ctx.SaveChangesAsync();

        var svc = new CourseService(new Repository<Course>(ctx), ctx, _mapper);
        var result = await svc.GetByIdAsync(course.CourseId);

        Assert.NotNull(result);
        Assert.Single(result!.Lessons);
        Assert.Single(result.Quizzes);
    }

    [Fact]
    public async Task UpdateCourse_ReturnsUpdated()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(UpdateCourse_ReturnsUpdated));
        ctx.Users.Add(new User { UserId = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "x" });
        ctx.Courses.Add(new Course { CourseId = 1, Title = "Old", Description = "Old desc", CreatedBy = 1 });
        await ctx.SaveChangesAsync();

        var svc = new CourseService(new Repository<Course>(ctx), ctx, _mapper);
        var result = await svc.UpdateAsync(1, new CourseUpdateDto("New Title", "New desc"));

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
    }

    [Fact]
    public async Task DeleteCourse_ReturnsTrue()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(DeleteCourse_ReturnsTrue));
        ctx.Users.Add(new User { UserId = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "x" });
        ctx.Courses.Add(new Course { CourseId = 1, Title = "ToDelete", Description = "x", CreatedBy = 1 });
        await ctx.SaveChangesAsync();

        var svc = new CourseService(new Repository<Course>(ctx), ctx, _mapper);
        Assert.True(await svc.DeleteAsync(1));
        Assert.Null(await svc.GetByIdAsync(1));
    }

    [Fact]
    public async Task DeleteCourse_NotFound_ReturnsFalse()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(DeleteCourse_NotFound_ReturnsFalse));
        var svc = new CourseService(new Repository<Course>(ctx), ctx, _mapper);
        Assert.False(await svc.DeleteAsync(999));
    }
}

public class QuizServiceTests
{
    private readonly IMapper _mapper;

    public QuizServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
    }

    [Fact]
    public async Task SubmitQuiz_ScoresCorrectly()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(SubmitQuiz_ScoresCorrectly));
        ctx.Users.Add(new User { UserId = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "x" });
        ctx.Courses.Add(new Course { CourseId = 1, Title = "C#", Description = "x", CreatedBy = 1 });
        var quiz = new Quiz { CourseId = 1, Title = "Quiz 1" };
        ctx.Quizzes.Add(quiz);
        await ctx.SaveChangesAsync();

        ctx.Questions.AddRange(
            new Question { QuizId = quiz.QuizId, QuestionText = "Q1", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "A" },
            new Question { QuizId = quiz.QuizId, QuestionText = "Q2", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "B" },
            new Question { QuizId = quiz.QuizId, QuestionText = "Q3", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "C" }
        );
        await ctx.SaveChangesAsync();

        var questions = ctx.Questions.Where(q => q.QuizId == quiz.QuizId).OrderBy(q => q.QuestionId).ToList();
        var svc = new QuizService(
            new Repository<Quiz>(ctx), new Repository<Question>(ctx),
            new Repository<Result>(ctx), ctx, _mapper);

        var result = await svc.SubmitAsync(quiz.QuizId, new QuizSubmitDto(1, questions.Select((q, i) => 
            new QuizAnswerDto(q.QuestionId, i switch { 0 => "A", 1 => "C", 2 => "C", _ => "" })).ToList()));

        Assert.Equal(2, result.Score);
    }

    [Fact]
    public async Task SubmitQuiz_InvalidQuiz_ThrowsKeyNotFound()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(SubmitQuiz_InvalidQuiz_ThrowsKeyNotFound));
        var svc = new QuizService(
            new Repository<Quiz>(ctx), new Repository<Question>(ctx),
            new Repository<Result>(ctx), ctx, _mapper);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.SubmitAsync(999, new QuizSubmitDto(1, new List<QuizAnswerDto>())));
    }

    [Fact]
    public async Task SubmitQuiz_NoQuestions_ThrowsInvalidOperation()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(SubmitQuiz_NoQuestions_ThrowsInvalidOperation));
        ctx.Courses.Add(new Course { CourseId = 1, Title = "C#", Description = "x", CreatedBy = 1 });
        ctx.Quizzes.Add(new Quiz { QuizId = 1, CourseId = 1, Title = "Empty Quiz" });
        await ctx.SaveChangesAsync();

        var svc = new QuizService(
            new Repository<Quiz>(ctx), new Repository<Question>(ctx),
            new Repository<Result>(ctx), ctx, _mapper);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SubmitAsync(1, new QuizSubmitDto(1, new List<QuizAnswerDto>())));
    }

    [Fact]
    public async Task GetResultsByUser_ReturnsFilteredResults()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(GetResultsByUser_ReturnsFilteredResults));
        ctx.Users.AddRange(
            new User { UserId = 1, FullName = "A", Email = "a@a.com", PasswordHash = "x" },
            new User { UserId = 2, FullName = "B", Email = "b@b.com", PasswordHash = "x" });
        ctx.Courses.Add(new Course { CourseId = 1, Title = "C#", Description = "x", CreatedBy = 1 });
        ctx.Quizzes.Add(new Quiz { QuizId = 1, CourseId = 1, Title = "Q1" });
        ctx.Results.AddRange(
            new Result { UserId = 1, QuizId = 1, Score = 80 },
            new Result { UserId = 1, QuizId = 1, Score = 90 },
            new Result { UserId = 2, QuizId = 1, Score = 70 });
        await ctx.SaveChangesAsync();

        var svc = new QuizService(
            new Repository<Quiz>(ctx), new Repository<Question>(ctx),
            new Repository<Result>(ctx), ctx, _mapper);

        var results = (await svc.GetResultsByUserAsync(1)).ToList();
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(1, r.UserId));
    }
}

public class LinqFilterTests
{
    [Fact]
    public async Task FindAsync_FiltersCorrectly()
    {
        using var ctx = TestDbHelper.CreateContext(nameof(FindAsync_FiltersCorrectly));
        ctx.Courses.AddRange(
            new Course { Title = "C# Basics", Description = "x", CreatedBy = 1 },
            new Course { Title = "Java Basics", Description = "x", CreatedBy = 1 },
            new Course { Title = "C# Advanced", Description = "x", CreatedBy = 1 });
        await ctx.SaveChangesAsync();

        var repo = new Repository<Course>(ctx);
        var results = (await repo.FindAsync(c => c.Title.Contains("C#"))).ToList();

        Assert.Equal(2, results.Count);
    }
}
