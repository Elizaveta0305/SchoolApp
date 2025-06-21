using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Linq;

namespace SchoolApplication.Tests
{
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;

            var context = new ApplicationDbContext(options);

            return context;
        }

        public void SeedData(ApplicationDbContext context, params object[] entities)
        {
            var newRoles = entities.OfType<Role>().ToList();
            foreach (var role in newRoles)
            {
                if (!context.Roles.Any(r => r.RoleID == role.RoleID))
                {
                    context.Roles.Add(role);
                }
            }
            context.SaveChanges();

            var newUsers = entities.OfType<User>().ToList();
            foreach (var user in newUsers)
            {
                if (!context.Users.Any(u => u.UserID == user.UserID))
                {
                    context.Users.Add(user);
                }
            }
            context.SaveChanges();

            var newGroups = entities.OfType<Group>().ToList();
            foreach (var group in newGroups)
            {
                if (!context.Groups.Any(g => g.GroupID == group.GroupID))
                {
                    context.Groups.Add(group);
                }
            }
            context.SaveChanges();

            var newSubjects = entities.OfType<Subject>().ToList();
            foreach (var subject in newSubjects)
            {
                if (!context.Subjects.Any(s => s.SubjectID == subject.SubjectID))
                {
                    context.Subjects.Add(subject);
                }
            }
            context.SaveChanges();

            var newClassrooms = entities.OfType<Classroom>().ToList();
            foreach (var classroom in newClassrooms)
            {
                if (!context.Classrooms.Any(c => c.ClassroomID == classroom.ClassroomID))
                {
                    context.Classrooms.Add(classroom);
                }
            }
            context.SaveChanges();

            var newStudyGroups = entities.OfType<StudyGroup>().ToList();
            foreach (var studyGroup in newStudyGroups)
            {
                if (!context.StudyGroups.Any(sg => sg.StudyGroupID == studyGroup.StudyGroupID))
                {
                    context.StudyGroups.Add(studyGroup);
                }
            }
            context.SaveChanges();

            var newLessons = entities.OfType<Lesson>().ToList();
            foreach (var lesson in newLessons)
            {
                if (!context.Lessons.Any(l => l.LessonID == lesson.LessonID))
                {
                    context.Lessons.Add(lesson);
                }
            }
            context.SaveChanges();

            var newAcademicPerformances = entities.OfType<AcademicPerformance>().ToList();
            foreach (var ap in newAcademicPerformances)
            {
                if (!context.AcademicPerformance.Any(a => a.PerformanceID == ap.PerformanceID))
                {
                    context.AcademicPerformance.Add(ap);
                }
            }
            context.SaveChanges();

            // В InMemory нет ChangeTracker.Clear(), но можно отсоединить сущности:
            foreach (var entry in context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
