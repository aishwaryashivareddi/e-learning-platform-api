using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs;

// ── User ──
public record UserRegisterDto(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record UserDto(int UserId, string FullName, string Email, DateTime CreatedAt);

public record UserUpdateDto(
    [Required, StringLength(100)] string FullName,
    [EmailAddress] string? Email);

// ── Course ──
public record CourseCreateDto(
    [Required, StringLength(200)] string Title,
    [Required] string Description,
    [Required] int CreatedBy);

public record CourseUpdateDto(
    [Required, StringLength(200)] string Title,
    [Required] string Description);

public record CourseDto(int CourseId, string Title, string Description, int CreatedBy, DateTime CreatedAt);

public record CourseDetailDto(int CourseId, string Title, string Description, int CreatedBy, DateTime CreatedAt,
    List<LessonDto> Lessons, List<QuizSummaryDto> Quizzes);

// ── Lesson ──
public record LessonCreateDto(
    [Required] int CourseId,
    [Required, StringLength(200)] string Title,
    [Required] string Content,
    int OrderIndex);

public record LessonUpdateDto(
    [Required, StringLength(200)] string Title,
    [Required] string Content,
    int OrderIndex);

public record LessonDto(int LessonId, int CourseId, string Title, string Content, int OrderIndex);

// ── Quiz ──
public record QuizCreateDto(
    [Required] int CourseId,
    [Required, StringLength(200)] string Title);

public record QuizSummaryDto(int QuizId, int CourseId, string Title);

public record QuizDetailDto(int QuizId, int CourseId, string Title, List<QuestionDto> Questions);

// ── Question ──
public record QuestionCreateDto(
    [Required] int QuizId,
    [Required] string QuestionText,
    [Required] string OptionA,
    [Required] string OptionB,
    [Required] string OptionC,
    [Required] string OptionD,
    [Required] string CorrectAnswer);

public record QuestionDto(int QuestionId, int QuizId, string QuestionText,
    string OptionA, string OptionB, string OptionC, string OptionD);

// ── Quiz Submission ──
public record QuizAnswerDto(int QuestionId, string SelectedAnswer);

public record QuizSubmitDto(
    [Required] int UserId,
    [Required] List<QuizAnswerDto> Answers);

// ── Result ──
public record ResultDto(int ResultId, int UserId, int QuizId, int Score, DateTime AttemptDate, string? QuizTitle);
