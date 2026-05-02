using LearningPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();
    public DbSet<ModuleCompletion> ModuleCompletions => Set<ModuleCompletion>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyPlanItem> StudyPlanItems => Set<StudyPlanItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<ModuleCompletion>()
            .HasIndex(m => new { m.UserId, m.ModuleId })
            .IsUnique();

        modelBuilder.Entity<StudyPlan>()
            .HasIndex(s => s.UserId)
            .IsUnique();
    }
}
