using AutoMapper;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;

namespace ELearning.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>();

        // Course
        CreateMap<CourseCreateDto, Course>();
        CreateMap<Course, CourseDto>();
        CreateMap<Course, CourseDetailDto>()
            .ForMember(d => d.Lessons, o => o.MapFrom(s => s.Lessons))
            .ForMember(d => d.Quizzes, o => o.MapFrom(s => s.Quizzes));

        // Lesson
        CreateMap<LessonCreateDto, Lesson>();
        CreateMap<Lesson, LessonDto>();

        // Quiz
        CreateMap<QuizCreateDto, Quiz>();
        CreateMap<Quiz, QuizSummaryDto>();
        CreateMap<Quiz, QuizDetailDto>()
            .ForMember(d => d.Questions, o => o.MapFrom(s => s.Questions));

        // Question
        CreateMap<QuestionCreateDto, Question>();
        CreateMap<Question, QuestionDto>();
    }
}
