using AutoMapper;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly IRepository<Quiz> _quizRepo;
    private readonly IRepository<Question> _questionRepo;
    private readonly IRepository<Result> _resultRepo;
    private readonly ELearningDbContext _context;
    private readonly IMapper _mapper;

    public QuizService(
        IRepository<Quiz> quizRepo,
        IRepository<Question> questionRepo,
        IRepository<Result> resultRepo,
        ELearningDbContext context,
        IMapper mapper)
    {
        _quizRepo = quizRepo;
        _questionRepo = questionRepo;
        _resultRepo = resultRepo;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuizSummaryDto>> GetByCourseIdAsync(int courseId)
    {
        var quizzes = await _quizRepo.FindAsync(q => q.CourseId == courseId);
        return _mapper.Map<IEnumerable<QuizSummaryDto>>(quizzes);
    }

    public async Task<QuizDto> CreateAsync(QuizCreateDto dto)
    {
        var quiz = _mapper.Map<Quiz>(dto);
        await _quizRepo.AddAsync(quiz);
        await _quizRepo.SaveAsync();
        return new QuizDto(quiz.QuizId, quiz.CourseId, quiz.Title);
    }

    public async Task<QuizDetailDto?> GetQuestionsAsync(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuizId == quizId);

        return quiz == null ? null : _mapper.Map<QuizDetailDto>(quiz);
    }

    public async Task<QuestionDto> AddQuestionAsync(QuestionCreateDto dto)
    {
        var quiz = await _quizRepo.GetByIdAsync(dto.QuizId);
        if (quiz == null) throw new KeyNotFoundException($"Quiz {dto.QuizId} not found.");

        var question = _mapper.Map<Question>(dto);
        await _questionRepo.AddAsync(question);
        await _questionRepo.SaveAsync();
        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<ResultDto> SubmitAsync(int quizId, QuizSubmitDto dto)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == quizId);

        if (quiz == null) throw new KeyNotFoundException($"Quiz {quizId} not found.");
        if (!quiz.Questions.Any()) throw new InvalidOperationException("Quiz has no questions.");

        var questionMap = quiz.Questions.ToDictionary(q => q.QuestionId);
        int score = dto.Answers.Count(a =>
            questionMap.TryGetValue(a.QuestionId, out var q) &&
            string.Equals(a.SelectedAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase));

        var result = new Result
        {
            UserId = dto.UserId,
            QuizId = quizId,
            Score = score,
            AttemptDate = DateTime.UtcNow
        };

        await _resultRepo.AddAsync(result);
        await _resultRepo.SaveAsync();

        return new ResultDto(result.ResultId, result.UserId, result.QuizId, result.Score, result.AttemptDate, quiz.Title);
    }

    public async Task<IEnumerable<ResultDto>> GetResultsByUserAsync(int userId)
    {
        var results = await _context.Results
            .Include(r => r.Quiz)
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.AttemptDate)
            .ToListAsync();

        return results.Select(r => new ResultDto(
            r.ResultId, r.UserId, r.QuizId, r.Score, r.AttemptDate, r.Quiz?.Title));
    }
}
