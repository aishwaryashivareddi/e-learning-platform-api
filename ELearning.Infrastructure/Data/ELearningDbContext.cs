using ELearning.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Data;

public class ELearningDbContext : DbContext
{
    public ELearningDbContext(DbContextOptions<ELearningDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // User
        mb.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        });

        // Course — One User → Many Courses
        mb.Entity<Course>(e =>
        {
            e.HasKey(c => c.CourseId);
            e.Property(c => c.Title).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).IsRequired();
            e.HasOne(c => c.Creator)
             .WithMany(u => u.Courses)
             .HasForeignKey(c => c.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Lesson — One Course → Many Lessons
        mb.Entity<Lesson>(e =>
        {
            e.HasKey(l => l.LessonId);
            e.Property(l => l.Title).HasMaxLength(200).IsRequired();
            e.Property(l => l.Content).IsRequired();
            e.HasOne(l => l.Course)
             .WithMany(c => c.Lessons)
             .HasForeignKey(l => l.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Quiz — One Course → Many Quizzes
        mb.Entity<Quiz>(e =>
        {
            e.HasKey(q => q.QuizId);
            e.Property(q => q.Title).HasMaxLength(200).IsRequired();
            e.HasOne(q => q.Course)
             .WithMany(c => c.Quizzes)
             .HasForeignKey(q => q.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Question — One Quiz → Many Questions
        mb.Entity<Question>(e =>
        {
            e.HasKey(q => q.QuestionId);
            e.Property(q => q.QuestionText).IsRequired();
            e.Property(q => q.CorrectAnswer).HasMaxLength(1).IsRequired();
            e.HasOne(q => q.Quiz)
             .WithMany(qz => qz.Questions)
             .HasForeignKey(q => q.QuizId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Result — One User → Many Results
        mb.Entity<Result>(e =>
        {
            e.HasKey(r => r.ResultId);
            e.HasOne(r => r.User)
             .WithMany(u => u.Results)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Quiz)
             .WithMany()
             .HasForeignKey(r => r.QuizId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
