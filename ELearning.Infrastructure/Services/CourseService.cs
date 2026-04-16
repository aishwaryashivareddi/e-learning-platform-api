using AutoMapper;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly IRepository<Course> _repo;
    private readonly ELearningDbContext _context;
    private readonly IMapper _mapper;

    public CourseService(IRepository<Course> repo, ELearningDbContext context, IMapper mapper)
    {
        _repo = repo;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses = await _context.Courses.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDetailDto?> GetByIdAsync(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Include(c => c.Quizzes)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == id);

        return course == null ? null : _mapper.Map<CourseDetailDto>(course);
    }

    public async Task<CourseDto> CreateAsync(CourseCreateDto dto)
    {
        var course = _mapper.Map<Course>(dto);
        course.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(course);
        await _repo.SaveAsync();
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course == null) return null;

        course.Title = dto.Title;
        course.Description = dto.Description;
        _repo.Update(course);
        await _repo.SaveAsync();
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course == null) return false;

        _repo.Remove(course);
        await _repo.SaveAsync();
        return true;
    }
}
