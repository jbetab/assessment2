using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de Course
        builder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).HasConversion<string>();
            entity.HasQueryFilter(c => !c.IsDeleted);
            
            entity.HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Lesson
        builder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
            entity.HasQueryFilter(l => !l.IsDeleted);
            
            entity.HasIndex(l => new { l.CourseId, l.Order }).IsUnique();
        });

        // Seed de usuario de prueba
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            NormalizedUserName = "TEST@EXAMPLE.COM",
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        user.PasswordHash = hasher.HashPassword(user, "Test123!");

        builder.Entity<IdentityUser>().HasData(user);
    }
}