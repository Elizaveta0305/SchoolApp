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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var rolesToAdd = new List<Role>();
            var usersToAdd = new List<User>();
            var groupsToAdd = new List<Group>();
            var subjectsToAdd = new List<Subject>();
            var studyGroupsToAdd = new List<StudyGroup>();
            var lessonsToAdd = new List<Lesson>();
            var academicPerformancesToAdd = new List<AcademicPerformance>();
            var classroomsToAdd = new List<Classroom>();

            foreach (var entity in entities)
            {
                if (entity is Role role) rolesToAdd.Add(role);
                else if (entity is User user) usersToAdd.Add(user);
                else if (entity is Group group) groupsToAdd.Add(group);
                else if (entity is Subject subject) subjectsToAdd.Add(subject);
                else if (entity is StudyGroup studyGroup) studyGroupsToAdd.Add(studyGroup);
                else if (entity is Lesson lesson) lessonsToAdd.Add(lesson);
                else if (entity is AcademicPerformance performance) academicPerformancesToAdd.Add(performance);
                else if (entity is Classroom classroom) classroomsToAdd.Add(classroom);
            }

            foreach (var role in rolesToAdd.DistinctBy(r => r.RoleID))
            {
                if (context.Entry(role).State == EntityState.Detached)
                {
                    context.Roles.Add(role);
                }
            }
            context.SaveChanges();

            foreach (var user in usersToAdd.DistinctBy(u => u.UserID))
            {
                if (user.Role != null && context.Entry(user.Role).State == EntityState.Detached)
                {
                    context.Roles.Attach(user.Role);
                }
                if (context.Entry(user).State == EntityState.Detached)
                {
                    context.Users.Add(user);
                }
            }
            context.SaveChanges();

            foreach (var subject in subjectsToAdd.DistinctBy(s => s.SubjectID))
            {
                if (context.Entry(subject).State == EntityState.Detached)
                {
                    context.Subjects.Add(subject);
                }
            }
            context.SaveChanges();

            foreach (var group in groupsToAdd.DistinctBy(g => g.GroupID))
            {
                if (group.Users != null)
                {
                    foreach (var userInGroup in group.Users.ToList())
                    {
                        if (context.Entry(userInGroup).State == EntityState.Detached)
                        {
                            context.Users.Attach(userInGroup);
                        }
                    }
                }
                if (context.Entry(group).State == EntityState.Detached)
                {
                    context.Groups.Add(group);
                }
            }
            context.SaveChanges();

            foreach (var classroom in classroomsToAdd.DistinctBy(c => c.ClassroomID))
            {
                if (context.Entry(classroom).State == EntityState.Detached)
                {
                    context.Classrooms.Add(classroom);
                }
            }
            context.SaveChanges();

            foreach (var studyGroup in studyGroupsToAdd.DistinctBy(sg => sg.StudyGroupID))
            {
                if (studyGroup.Teacher != null && context.Entry(studyGroup.Teacher).State == EntityState.Detached)
                {
                    context.Users.Attach(studyGroup.Teacher);
                }
                if (studyGroup.Group != null && context.Entry(studyGroup.Group).State == EntityState.Detached)
                {
                    context.Groups.Attach(studyGroup.Group);
                }
                if (studyGroup.Subject != null && context.Entry(studyGroup.Subject).State == EntityState.Detached)
                {
                    context.Subjects.Attach(studyGroup.Subject);
                }

                if (context.Entry(studyGroup).State == EntityState.Detached)
                {
                    context.StudyGroups.Add(studyGroup);
                }
            }
            context.SaveChanges();

            foreach (var lesson in lessonsToAdd.DistinctBy(l => l.LessonID))
            {
                if (lesson.StudyGroup != null && context.Entry(lesson.StudyGroup).State == EntityState.Detached)
                {
                    context.StudyGroups.Attach(lesson.StudyGroup);
                }
                if (lesson.Classroom != null && context.Entry(lesson.Classroom).State == EntityState.Detached)
                {
                    context.Classrooms.Attach(lesson.Classroom);
                }
                if (context.Entry(lesson).State == EntityState.Detached)
                {
                    context.Lessons.Add(lesson);
                }
            }
            context.SaveChanges();

            foreach (var performance in academicPerformancesToAdd.DistinctBy(ap => ap.PerformanceID))
            {
                if (performance.Student != null && context.Entry(performance.Student).State == EntityState.Detached)
                {
                    context.Users.Attach(performance.Student);
                }
                if (performance.Lesson != null && context.Entry(performance.Lesson).State == EntityState.Detached)
                {
                    context.Lessons.Attach(performance.Lesson);
                }
                if (context.Entry(performance).State == EntityState.Detached)
                {
                    context.AcademicPerformance.Add(performance);
                }
            }
            context.SaveChanges();

            context.ChangeTracker.Clear();
        }
    }
}