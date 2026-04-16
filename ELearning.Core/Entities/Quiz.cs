namespace ELearning.Core.Entities;

public class Quiz
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;

    public Course Course { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
