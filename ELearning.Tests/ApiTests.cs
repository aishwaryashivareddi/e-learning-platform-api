using System.Net;
using System.Net.Http.Json;
using ELearning.Core.DTOs;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.Tests;

public class TestWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL EF registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ELearningDbContext>)
                         || d.ServiceType == typeof(ELearningDbContext)
                         || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddDbContext<ELearningDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}

public class ApiTests : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client;

    public ApiTests(TestWebFactory factory) => _client = factory.CreateClient();

    // ── User API ──

    [Fact]
    public async Task RegisterUser_Returns201()
    {
        var res = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "Test User", email = $"t{Guid.NewGuid()}@test.com", password = "Pass123!" });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var user = await res.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.True(user!.UserId > 0);
        Assert.Equal("Test User", user.FullName);
    }

    [Fact]
    public async Task GetUser_NotFound_Returns404()
    {
        var res = await _client.GetAsync("/api/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ── Course API ──

    [Fact]
    public async Task GetAllCourses_Returns200()
    {
        var res = await _client.GetAsync("/api/courses");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetCourse_Returns201And200()
    {
        // Create user
        var userRes = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "Creator", email = $"c{Guid.NewGuid()}@test.com", password = "Pass123!" });
        var user = await userRes.Content.ReadFromJsonAsync<UserDto>();

        // Create course
        var createRes = await _client.PostAsJsonAsync("/api/courses",
            new { title = "API Test Course", description = "Desc", createdBy = user!.UserId });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var course = await createRes.Content.ReadFromJsonAsync<CourseDto>();
        Assert.Equal("API Test Course", course!.Title);

        // Get by id
        var getRes = await _client.GetAsync($"/api/courses/{course.CourseId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
    }

    [Fact]
    public async Task UpdateCourse_Returns200()
    {
        var userRes = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "U", email = $"u{Guid.NewGuid()}@test.com", password = "Pass123!" });
        var user = await userRes.Content.ReadFromJsonAsync<UserDto>();

        var createRes = await _client.PostAsJsonAsync("/api/courses",
            new { title = "Old Title", description = "Old", createdBy = user!.UserId });
        var course = await createRes.Content.ReadFromJsonAsync<CourseDto>();

        var updateRes = await _client.PutAsJsonAsync($"/api/courses/{course!.CourseId}",
            new { title = "New Title", description = "New" });
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updated = await updateRes.Content.ReadFromJsonAsync<CourseDto>();
        Assert.Equal("New Title", updated!.Title);
    }

    [Fact]
    public async Task DeleteCourse_Returns204_ThenGetReturns404()
    {
        var userRes = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "D", email = $"d{Guid.NewGuid()}@test.com", password = "Pass123!" });
        var user = await userRes.Content.ReadFromJsonAsync<UserDto>();

        var createRes = await _client.PostAsJsonAsync("/api/courses",
            new { title = "ToDelete", description = "x", createdBy = user!.UserId });
        var course = await createRes.Content.ReadFromJsonAsync<CourseDto>();

        var delRes = await _client.DeleteAsync($"/api/courses/{course!.CourseId}");
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getRes = await _client.GetAsync($"/api/courses/{course.CourseId}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    // ── Quiz API ──

    [Fact]
    public async Task QuizSubmit_ScoresCorrectly_Returns200()
    {
        var userRes = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "Quizzer", email = $"q{Guid.NewGuid()}@test.com", password = "Pass123!" });
        var user = await userRes.Content.ReadFromJsonAsync<UserDto>();

        var courseRes = await _client.PostAsJsonAsync("/api/courses",
            new { title = "QC", description = "x", createdBy = user!.UserId });
        var course = await courseRes.Content.ReadFromJsonAsync<CourseDto>();

        var quizRes = await _client.PostAsJsonAsync("/api/quizzes",
            new { courseId = course!.CourseId, title = "Quiz" });
        var quiz = await quizRes.Content.ReadFromJsonAsync<QuizSummaryDto>();

        var q1Res = await _client.PostAsJsonAsync("/api/questions",
            new { quizId = quiz!.QuizId, questionText = "Q1", optionA = "A", optionB = "B", optionC = "C", optionD = "D", correctAnswer = "A" });
        var q1 = await q1Res.Content.ReadFromJsonAsync<QuestionDto>();

        var q2Res = await _client.PostAsJsonAsync("/api/questions",
            new { quizId = quiz.QuizId, questionText = "Q2", optionA = "A", optionB = "B", optionC = "C", optionD = "D", correctAnswer = "B" });
        var q2 = await q2Res.Content.ReadFromJsonAsync<QuestionDto>();

        // Submit: Q1 correct, Q2 wrong
        var submitRes = await _client.PostAsJsonAsync($"/api/quizzes/{quiz.QuizId}/submit", new
        {
            userId = user.UserId,
            answers = new[] {
                new { questionId = q1!.QuestionId, selectedAnswer = "A" },
                new { questionId = q2!.QuestionId, selectedAnswer = "C" }
            }
        });
        Assert.Equal(HttpStatusCode.OK, submitRes.StatusCode);
        var result = await submitRes.Content.ReadFromJsonAsync<ResultDto>();
        Assert.Equal(1, result!.Score);
    }

    [Fact]
    public async Task QuizSubmit_InvalidQuiz_Returns404()
    {
        var res = await _client.PostAsJsonAsync("/api/quizzes/9999/submit",
            new { userId = 1, answers = new[] { new { questionId = 1, selectedAnswer = "A" } } });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetResults_Returns200()
    {
        var res = await _client.GetAsync("/api/results/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // ── Lesson API ──

    [Fact]
    public async Task CreateLesson_Returns201()
    {
        var userRes = await _client.PostAsJsonAsync("/api/users/register",
            new { fullName = "L", email = $"l{Guid.NewGuid()}@test.com", password = "Pass123!" });
        var user = await userRes.Content.ReadFromJsonAsync<UserDto>();

        var courseRes = await _client.PostAsJsonAsync("/api/courses",
            new { title = "LC", description = "x", createdBy = user!.UserId });
        var course = await courseRes.Content.ReadFromJsonAsync<CourseDto>();

        var res = await _client.PostAsJsonAsync("/api/lessons",
            new { courseId = course!.CourseId, title = "Lesson 1", content = "Content", orderIndex = 1 });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var getRes = await _client.GetAsync($"/api/courses/{course.CourseId}/lessons");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var lessons = await getRes.Content.ReadFromJsonAsync<List<LessonDto>>();
        Assert.Single(lessons!);
    }
}
