using ELearning.Core.DTOs;
using ELearning.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _svc;
    public LessonsController(ILessonService svc) => _svc = svc;

    [HttpGet("courses/{courseId}/lessons")]
    public async Task<IActionResult> GetByCourse(int courseId)
        => Ok(await _svc.GetByCourseIdAsync(courseId));

    [HttpPost("lessons")]
    public async Task<IActionResult> Create(LessonCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var lesson = await _svc.CreateAsync(dto);
        return Created($"/api/lessons/{lesson.LessonId}", lesson);
    }

    [HttpPut("lessons/{id}")]
    public async Task<IActionResult> Update(int id, LessonUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var lesson = await _svc.UpdateAsync(id, dto);
        return lesson == null ? NotFound() : Ok(lesson);
    }

    [HttpDelete("lessons/{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
