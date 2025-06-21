using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolApplication.Tests
{
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _databaseName;

        public TestDbContextFactory(string databaseName)
        {
            _databaseName = databaseName;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;
            return new ApplicationDbContext(options);
        }

        public void SeedData(ApplicationDbContext context, params object[] entities)
        {
            context.Database.EnsureCreated();

            var newRoles = entities.OfType<Role>().DistinctBy(r => r.RoleID).ToList();
            foreach (var role in newRoles)
            {
                if (context.Roles.Find(role.RoleID) == null)
                {
                    context.Roles.Add(role);
                }
            }
            context.SaveChanges();

            var newGroups = entities.OfType<Group>().DistinctBy(g => g.GroupID).ToList();
            foreach (var group in newGroups)
            {
                if (context.Groups.Find(group.GroupID) == null)
                {
                    context.Groups.Add(group);
                }
            }
            context.SaveChanges();

            var newSubjects = entities.OfType<Subject>().DistinctBy(s => s.SubjectID).ToList();
            foreach (var subject in newSubjects)
            {
                if (context.Subjects.Find(subject.SubjectID) == null)
                {
                    context.Subjects.Add(subject);
                }
            }
            context.SaveChanges();

            var newClassrooms = entities.OfType<Classroom>().DistinctBy(c => c.ClassroomID).ToList();
            foreach (var classroom in newClassrooms)
            {
                if (context.Classrooms.Find(classroom.ClassroomID) == null)
                {
                    context.Classrooms.Add(classroom);
                }
            }
            context.SaveChanges();

            var newUsers = entities.OfType<User>().DistinctBy(u => u.UserID).ToList();
            foreach (var user in newUsers)
            {
                if (context.Users.Find(user.UserID) == null)
                {
                    context.Users.Add(user);
                }
                else
                {
                    context.Entry(user).State = EntityState.Modified;
                }
            }
            context.SaveChanges();


            var newStudyGroups = entities.OfType<StudyGroup>().DistinctBy(sg => sg.StudyGroupID).ToList();
            foreach (var studyGroup in newStudyGroups)
            {
                if (context.StudyGroups.Find(studyGroup.StudyGroupID) == null)
                {
                    context.StudyGroups.Add(studyGroup);
                }
            }
            context.SaveChanges();

            var newLessons = entities.OfType<Lesson>().DistinctBy(l => l.LessonID).ToList();
            foreach (var lesson in newLessons)
            {
                if (context.Lessons.Find(lesson.LessonID) == null)
                {
                    context.Lessons.Add(lesson);
                }
            }
            context.SaveChanges();

            var newAcademicPerformances = entities.OfType<AcademicPerformance>().DistinctBy(ap => ap.PerformanceID).ToList();
            foreach (var ap in newAcademicPerformances)
            {
                if (context.AcademicPerformance.Find(ap.PerformanceID) == null)
                {
                    context.AcademicPerformance.Add(ap);
                }
            }
            context.SaveChanges();

            context.ChangeTracker.Clear();
        }
    }
}