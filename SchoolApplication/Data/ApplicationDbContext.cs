using Microsoft.EntityFrameworkCore;
using SchoolApplication.Models;
using System;

namespace SchoolApplication.Data
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<StudyGroup> StudyGroups { get; set; } = null!;
        public DbSet<Classroom> Classrooms { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<AcademicPerformance> AcademicPerformance { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь User с Role (у пользователя одна роль, у роли много пользователей)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            // Связь User с Group (у студента одна группа, у группы много студентов)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GroupID);

            // Связь StudyGroup с Teacher (Учителем является User)
            modelBuilder.Entity<StudyGroup>()
                .HasOne(sg => sg.Teacher)
                .WithMany(u => u.StudyGroupsAsTeacher)
                .HasForeignKey(sg => sg.TeacherID)
                .OnDelete(DeleteBehavior.NoAction);

            // Связь AcademicPerformance
            modelBuilder.Entity<AcademicPerformance>()
                .HasOne(ap => ap.Student)
                .WithMany(u => u.AcademicPerformanceAsStudent) 
                .HasForeignKey(ap => ap.StudentID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}