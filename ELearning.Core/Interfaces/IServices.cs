using ELearning.Core.DTOs;

namespace ELearning.Core.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(UserRegisterDto dto);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> UpdateAsync(int id, UserUpdateDto dto);
}

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync();
    Task<CourseDetailDto?> GetByIdAsync(int id);
    Task<CourseDto> CreateAsync(CourseCreateDto dto);
    Task<CourseDto?> UpdateAsync(int id, CourseUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface ILessonService
{
    Task<IEnumerable<LessonDto>> GetByCourseIdAsync(int courseId);
    Task<LessonDto> CreateAsync(LessonCreateDto dto);
    Task<LessonDto?> UpdateAsync(int id, LessonUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IQuizService
{
    Task<IEnumerable<QuizSummaryDto>> GetByCourseIdAsync(int courseId);
    Task<QuizDto> CreateAsync(QuizCreateDto dto);
    Task<QuizDetailDto?> GetQuestionsAsync(int quizId);
    Task<QuestionDto> AddQuestionAsync(QuestionCreateDto dto);
    Task<ResultDto> SubmitAsync(int quizId, QuizSubmitDto dto);
    Task<IEnumerable<ResultDto>> GetResultsByUserAsync(int userId);
}

public record QuizDto(int QuizId, int CourseId, string Title);
