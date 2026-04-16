using ELearning.Core.DTOs;
using ELearning.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _svc;
    public CoursesController(ICourseService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await _svc.GetByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var course = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, course);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CourseUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var course = await _svc.UpdateAsync(id, dto);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
