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
        public DbSet<AcademicPerformance> AcademicPerformances { get; set; } = null!;

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

            // Пользователи:
            //  Пароли здесь хешируется! BCrypt
            string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");    // Пароль для admin123
            string teacherPasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"); // Пароль для teacher123
            string studentPasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"); // Пароль для student123

            modelBuilder.Entity<User>().HasData(
                new User { UserID = 4, FirstName = "Admin", LastName = "Admin", MiddleName = "Admin", BirthDate = new DateTime(2000, 1, 1), Email = "admin@school.com", Phone = "89531253412", RoleID = 1, GroupID = null, Username = "admin123", PasswordHash = adminPasswordHash },
                new User { UserID = 5, FirstName = "Мария", LastName = "Петрова", MiddleName = "Сергеевна", BirthDate = new DateTime(1990, 5, 10), Email = "teacher@school.com", Phone = "89425674323", RoleID = 2, GroupID = null, Username = "teacher123", PasswordHash = teacherPasswordHash },
                new User { UserID = 6, FirstName = "Алексей", LastName = "Сидоров", MiddleName = "Павлович", BirthDate = new DateTime(2005, 9, 15), Email = "student1@school.com", Phone = "89357454512", RoleID = 3, GroupID = 1, Username = "student123", PasswordHash = studentPasswordHash },
                new User { UserID = 7, FirstName = "Елена", LastName = "Кузнецова", MiddleName = "Дмитриевна", BirthDate = new DateTime(2006, 3, 20), Email = "student2@school.com", Phone = "89046492495", RoleID = 3, GroupID = 2, Username = "student234", PasswordHash = studentPasswordHash }
            );
        }
    }
}