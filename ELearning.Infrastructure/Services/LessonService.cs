using AutoMapper;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;

namespace ELearning.Infrastructure.Services;

public class LessonService : ILessonService
{
    private readonly IRepository<Lesson> _repo;
    private readonly IMapper _mapper;

    public LessonService(IRepository<Lesson> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LessonDto>> GetByCourseIdAsync(int courseId)
    {
        var lessons = await _repo.FindAsync(l => l.CourseId == courseId);
        return _mapper.Map<IEnumerable<LessonDto>>(lessons.OrderBy(l => l.OrderIndex));
    }

    public async Task<LessonDto> CreateAsync(LessonCreateDto dto)
    {
        var lesson = _mapper.Map<Lesson>(dto);
        await _repo.AddAsync(lesson);
        await _repo.SaveAsync();
        return _mapper.Map<LessonDto>(lesson);
    }

    public async Task<LessonDto?> UpdateAsync(int id, LessonUpdateDto dto)
    {
        var lesson = await _repo.GetByIdAsync(id);
        if (lesson == null) return null;

        lesson.Title = dto.Title;
        lesson.Content = dto.Content;
        lesson.OrderIndex = dto.OrderIndex;
        _repo.Update(lesson);
        await _repo.SaveAsync();
        return _mapper.Map<LessonDto>(lesson);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lesson = await _repo.GetByIdAsync(id);
        if (lesson == null) return false;

        _repo.Remove(lesson);
        await _repo.SaveAsync();
        return true;
    }
}
