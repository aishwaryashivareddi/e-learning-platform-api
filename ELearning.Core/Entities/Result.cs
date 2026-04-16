namespace ELearning.Core.Entities;

public class Result
{
    public int ResultId { get; set; }
    public int UserId { get; set; }
    public int QuizId { get; set; }
    public int Score { get; set; }
    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
}
