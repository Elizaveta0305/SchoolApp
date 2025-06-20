using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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

            foreach (var entity in entities)
            {
                if (entity is Role role) context.Roles.Add(role);
                else if (entity is User user) context.Users.Add(user);
                else if (entity is Group group) context.Groups.Add(group);
                else if (entity is Subject subject) context.Subjects.Add(subject);
                else if (entity is StudyGroup studyGroup) context.StudyGroups.Add(studyGroup);
                else if (entity is Lesson lesson) context.Lessons.Add(lesson);
                else if (entity is AcademicPerformance performance) context.AcademicPerformance.Add(performance);
            }
            context.SaveChanges();
            context.ChangeTracker.Clear();
        }
    }
}