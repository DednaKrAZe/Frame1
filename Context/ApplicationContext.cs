using Engineering.Models;
using Microsoft.EntityFrameworkCore;
using Task = Engineering.Models.Task;

namespace Engineering.Context;

public class ApplicationContext: DbContext
{
    public required DbSet<Defect> Defects { get; set; }
    public required DbSet<Project> Projects { get; set; }
    public required DbSet<Task> Tasks { get; set; }
    public required DbSet<User> Users { get; set; }
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Executor)
            .WithMany()
            .HasForeignKey(t => t.ExecutorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Defect)
            .WithMany()
            .HasForeignKey(t => t.DefectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}