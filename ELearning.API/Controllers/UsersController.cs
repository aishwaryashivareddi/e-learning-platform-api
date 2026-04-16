using ELearning.Core.DTOs;
using ELearning.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    public UsersController(IUserService svc) => _svc = svc;

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var user = await _svc.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _svc.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _svc.UpdateAsync(id, dto);
        return user == null ? NotFound() : Ok(user);
    }
}
