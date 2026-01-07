using EDzienik.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SubjectAssignment> SubjectAssignments => Set<SubjectAssignment>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<SchoolEvent> SchoolEvents => Set<SchoolEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .HasIndex(x => x.UserId).IsUnique();

        modelBuilder.Entity<Student>()
            .HasOne(x => x.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(x => x.UserId);

        modelBuilder.Entity<Teacher>()
            .HasIndex(x => x.UserId).IsUnique();

        modelBuilder.Entity<Teacher>()
            .HasOne(x => x.User)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(x => x.UserId);

        modelBuilder.Entity<SchoolClass>()
            .HasIndex(x => new { x.Name, x.SchoolYear })
            .IsUnique();

        modelBuilder.Entity<Subject>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<SubjectAssignment>()
            .HasIndex(x => new { x.TeacherId, x.SubjectId, x.SchoolClassId })
            .IsUnique();

        modelBuilder.Entity<Grade>()
            .HasOne(x => x.Student)
            .WithMany(s => s.Grades)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SchoolEvent>()
            .HasOne(x => x.SchoolClass)
            .WithMany(c => c.SchoolEvents)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
