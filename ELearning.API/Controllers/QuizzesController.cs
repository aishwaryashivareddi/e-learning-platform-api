using ELearning.Core.DTOs;
using ELearning.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _svc;
    public QuizzesController(IQuizService svc) => _svc = svc;

    [HttpGet("quizzes/{courseId}")]
    public async Task<IActionResult> GetByCourse(int courseId)
        => Ok(await _svc.GetByCourseIdAsync(courseId));

    [HttpPost("quizzes")]
    public async Task<IActionResult> Create(QuizCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var quiz = await _svc.CreateAsync(dto);
        return Created($"/api/quizzes/{quiz.QuizId}", quiz);
    }

    [HttpGet("quizzes/{quizId}/questions")]
    public async Task<IActionResult> GetQuestions(int quizId)
    {
        var quiz = await _svc.GetQuestionsAsync(quizId);
        return quiz == null ? NotFound() : Ok(quiz);
    }

    [HttpPost("questions")]
    public async Task<IActionResult> AddQuestion(QuestionCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var question = await _svc.AddQuestionAsync(dto);
            return Created($"/api/questions/{question.QuestionId}", question);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpPost("quizzes/{quizId}/submit")]
    public async Task<IActionResult> Submit(int quizId, QuizSubmitDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _svc.SubmitAsync(quizId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("results/{userId}")]
    public async Task<IActionResult> GetResults(int userId)
        => Ok(await _svc.GetResultsByUserAsync(userId));
}
