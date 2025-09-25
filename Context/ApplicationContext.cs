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
    
}