using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.Models.DisplayModels;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    public class HomeVmTests : IDisposable
    {
        private readonly TestDbContextFactory _dbFactory = new TestDbContextFactory();

        public void Dispose()
        {
            // Очистка ресурсов, если нужно
        }

        [Fact]
        public async Task Receive_WithValidUser_LoadsDataAndSetsWelcomeMessage()
        {
            // Arrange
            var user = new User
            {
                UserID = 1,
                FirstName = "Иван",
                GroupID = 1
            };
            var group = new Group { GroupID = 1, GroupName = "Группа 1" };
            var subject = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var teacher = new User { UserID = 2, FirstName = "Петр", LastName = "Иванов" };
            var studyGroup = new StudyGroup { StudyGroupID = 1, GroupID = 1, SubjectID = 1, TeacherID = 2, Subject = subject, Teacher = teacher };
            var classroom = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var lesson = new Lesson
            {
                LessonID = 1,
                StudyGroup = studyGroup,
                Classroom = classroom,
                LessonDate = DateTime.Today.AddDays(1),
                LessonTime = TimeSpan.FromHours(10)
            };
            var ap1 = new AcademicPerformance { PerformanceID = 1, StudentID = 1, Attendance = false, Grade = "4" };
            var ap2 = new AcademicPerformance { PerformanceID = 2, StudentID = 1, Attendance = true, Grade = "5" };

            var context = _dbFactory.CreateDbContext();
            _dbFactory.SeedData(context, user, group, subject, teacher, studyGroup, classroom, lesson, ap1, ap2);

            var vm = new HomeVm(_dbFactory);

            // Act
            vm.Receive(new UserAuthenticatedMessage(user));
            await vm.LoadAllHomeData(); // Ждем, чтобы async загрузка завершилась

            // Assert
            Assert.Contains("Иван", vm.WelcomeMessage);
            Assert.True(vm.UpcomingLessons.Any());
            Assert.Equal("Математика", vm.UpcomingLessons.First().SubjectName);
            Assert.Equal("Иванов П.", vm.UpcomingLessons.First().TeacherFullName);
            Assert.Equal("101", vm.UpcomingLessons.First().RoomNumber);

            Assert.Equal(1, vm.AbsencesCount); // одна пропущенная (Attendance == false)
            Assert.Equal("1 / 30", vm.AbsencesDisplayText);

            Assert.Equal(1, vm.SubjectsCount);

            Assert.True(vm.HasGradesToDisplay);
            Assert.InRange(vm.AverageGradeValue, 4.5 - 0.01, 4.5 + 0.01); // Среднее из 4 и 5 = 4.5
            Assert.Equal("4.50", vm.AverageGradeDisplayText);
        }

        [Fact]
        public void Receive_WithNullUser_ResetsProperties()
        {
            var vm = new HomeVm(_dbFactory);

            // Act
            vm.Receive(new UserAuthenticatedMessage(null));

            // Assert
            Assert.Equal("Добро пожаловать!", vm.WelcomeMessage);
            Assert.Empty(vm.UpcomingLessons);
            Assert.Equal(0, vm.AbsencesCount);
            Assert.Equal("0 / 30", vm.AbsencesDisplayText);
            Assert.Equal(0, vm.SubjectsCount);
            Assert.Equal(0.0, vm.AverageGradeValue);
            Assert.False(vm.HasGradesToDisplay);
            Assert.Equal("Неизвестно", vm.AcademicYear);
        }

        [Fact]
        public async Task LoadAllHomeData_WithNullUserOrGroup_SetsDefaultValues()
        {
            var vm = new HomeVm(_dbFactory);

            // Внутренний вызов - приватный, поэтому тестируем через Receive с пустым юзером
            vm.Receive(new UserAuthenticatedMessage(new User()));

            await Task.Delay(100);

            Assert.Equal("Добро пожаловать!", vm.WelcomeMessage);
            Assert.Empty(vm.UpcomingLessons);
            Assert.Equal(0, vm.AbsencesCount);
            Assert.Equal("0 / 30", vm.AbsencesDisplayText);
            Assert.Equal(0, vm.SubjectsCount);
            Assert.Equal(0.0, vm.AverageGradeValue);
            Assert.False(vm.HasGradesToDisplay);
            Assert.Equal("Неизвестно", vm.AcademicYear);
        }

        [Fact]
        public async Task LoadAnalyticsData_WithInvalidGrades_HandlesGracefully()
        {
            var user = new User { UserID = 1, GroupID = 1 };
            var ap1 = new AcademicPerformance { PerformanceID = 1, StudentID = 1, Attendance = true, Grade = "A" }; // не число
            var ap2 = new AcademicPerformance { PerformanceID = 2, StudentID = 1, Attendance = false, Grade = null };

            var context = _dbFactory.CreateDbContext();
            _dbFactory.SeedData(context, user, ap1, ap2);

            var vm = new HomeVm(_dbFactory);
            vm.Receive(new UserAuthenticatedMessage(user));

            await Task.Delay(100);

            Assert.False(vm.HasGradesToDisplay);
            Assert.Equal(0.0, vm.AverageGradeValue);
        }

        [Fact]
        public async Task LoadUpcomingLessonsInternal_WithNoGroup_DoesNotThrow()
        {
            var vm = new HomeVm(_dbFactory);

            var context = _dbFactory.CreateDbContext();

            // Пытаемся загрузить уроки без группы (метод приватный, тестируем через Receive)
            vm.Receive(new UserAuthenticatedMessage(new User { UserID = 1 }));

            await vm.LoadAllHomeData();

            // Если дойдет сюда без исключения — тест пройден
            Assert.True(true);
        }
    }
}
