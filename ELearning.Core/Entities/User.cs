namespace ELearning.Core.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Result> Results { get; set; } = new List<Result>();
}
