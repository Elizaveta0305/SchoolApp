using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.Models.DisplayModels;
using SchoolApplication.ViewModels; // Убедитесь, что это правильное пространство имен для HomeVm
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class HomeVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext;
        private IMessenger _messenger;
        private HomeVm _homeVm;

        private User _testUserStudent;
        private User _testUserTeacher;
        private Group _testGroup;
        private Subject _testSubject;
        private Classroom _testClassroom;
        private StudyGroup _testStudyGroup;
        private Role _studentRole;
        private Role _teacherRole;

        public HomeVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory(); // Создается новый экземпляр фабрики для каждого тестового класса
            _messenger = fixture.Messenger;
        }

        private void SetupTest()
        {
            _messenger = new StrongReferenceMessenger();
            _testDbContextFactory = new TestDbContextFactory();
            _currentTestDbContext = _testDbContextFactory.CreateDbContext(); // Создает новую пустую In-Memory DB для каждого теста

            // === 1. Создание базовых сущностей (Roles и Group) ===
            // Эти должны быть созданы первыми, так как на них ссылаются User и StudyGroup
            _studentRole = new Role { RoleID = 1, RoleName = "Student" };
            _teacherRole = new Role { RoleID = 2, RoleName = "Teacher" };
            _testGroup = new Group { GroupID = 1, GroupName = "Тестовая группа" }; // <-- Добавлена инициализация _testGroup

            // === 2. Создание пользователей ===
            _testUserStudent = new User
            {
                UserID = 1001,
                FirstName = "Студент",
                LastName = "Тестов",
                GroupID = _testGroup.GroupID, // <-- Присваиваем GroupID из созданной группы
                RoleID = _studentRole.RoleID
            };
            _testUserTeacher = new User
            {
                UserID = 1002,
                FirstName = "Учитель",
                LastName = "Преподават",
                RoleID = _teacherRole.RoleID
            };

            // === 3. Создание других необходимых сущностей ===
            _testSubject = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _testClassroom = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            _testStudyGroup = new StudyGroup
            {
                StudyGroupID = 1,
                GroupID = _testGroup.GroupID, // <-- Связываем с _testGroup
                SubjectID = _testSubject.SubjectID,
                TeacherID = _testUserTeacher.UserID
            };

            // === 4. ЗАСЕЙТЕ ВСЕ НЕОБХОДИМЫЕ СУЩНОСТИ В БАЗУ ДАННЫХ ===
            // Порядок важен: сначала независимые сущности (Role, Group, Subject, Classroom),
            // затем те, что на них ссылаются (User, StudyGroup),
            // затем те, что ссылаются на предыдущие (Lesson, AcademicPerformance - их лучше засеивать в каждом тесте, если они специфичны для него)
            _testDbContextFactory.SeedData(_currentTestDbContext,
                _studentRole,
                _teacherRole,
                _testGroup,        // <-- ОБЯЗАТЕЛЬНО ДОБАВЬТЕ _testGroup
                _testSubject,
                _testClassroom,
                _testUserStudent,  // Теперь _testUserStudent имеет GroupID и RoleID
                _testUserTeacher,  // Теперь _testUserTeacher имеет RoleID
                _testStudyGroup
            );

            // === 5. Инициализация HomeVm ===
            _homeVm = new HomeVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
        }

        [Fact]
        public async Task Receive_WithValidUser_LoadsDataAndSetsWelcomeMessage()
        {
            // Arrange
            SetupTest();

            var lesson = new Lesson
            {
                LessonID = 20001,
                StudyGroupID = _testStudyGroup.StudyGroupID,
                ClassroomID = _testClassroom.ClassroomID,
                LessonDate = DateTime.Today.AddDays(1),
                LessonTime = TimeSpan.FromHours(10),
                // Здесь можно добавить связанные объекты, если они не будут дублировать уже добавленные сущности.
                // В данном случае, StudyGroup и Classroom уже должны быть в БД.
                // Если нет проблем с их добавлением через StudyGroup, то можно оставить.
                // StudyGroup = _testStudyGroup,
                // Classroom = _testClassroom
            };

            // Убедитесь, что StudentID здесь соответствует _testUserStudent.UserID
            var ap1 = new AcademicPerformance { PerformanceID = 30001, StudentID = _testUserStudent.UserID, Attendance = false, Grade = "4", LessonID = lesson.LessonID }; // Пропуск
            var ap2 = new AcademicPerformance { PerformanceID = 30002, StudentID = _testUserStudent.UserID, Attendance = true, Grade = "5", LessonID = lesson.LessonID }; // Посещение

            // Явно добавляем Lesson и AcademicPerformance
            _testDbContextFactory.SeedData(_currentTestDbContext, lesson, ap1, ap2);

            // Act
            _messenger.Send(new UserAuthenticatedMessage(_testUserStudent));
            await Task.Delay(100);

            // Assert
            Assert.Contains(_testUserStudent.FirstName, _homeVm.WelcomeMessage);
            Assert.True(_homeVm.UpcomingLessons.Any());
            Assert.Equal(_testSubject.SubjectName, _homeVm.UpcomingLessons.First().SubjectName);
            Assert.Equal($"{_testUserTeacher.LastName} {_testUserTeacher.FirstName[0]}.", _homeVm.UpcomingLessons.First().TeacherFullName);
            Assert.Equal(_testClassroom.RoomNumber, _homeVm.UpcomingLessons.First().RoomNumber);

            // Теперь это должно быть 1
            Assert.Equal(1, _homeVm.AbsencesCount);
            Assert.Equal("1 / 30", _homeVm.AbsencesDisplayText);

            Assert.Equal(1, _homeVm.SubjectsCount);

            Assert.True(_homeVm.HasGradesToDisplay);
            Assert.InRange(_homeVm.AverageGradeValue, 4.5 - 0.01, 4.5 + 0.01);
            Assert.Equal("4.50", _homeVm.AverageGradeDisplayText);
        }

        [Fact]
        public async Task Receive_WithNullUser_ResetsProperties()
        {
            // Arrange
            SetupTest();

            _homeVm.WelcomeMessage = "Привет, Мир!";
            _homeVm.UpcomingLessons.Add(new LessonDisplayModel { LessonId = 999 });
            _homeVm.AbsencesCount = 5;
            _homeVm.SubjectsCount = 2;
            _homeVm.AverageGradeValue = 3.5;
            _homeVm.HasGradesToDisplay = true;
            _homeVm.AcademicYear = "2024-2025";

            // Act
            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            // Assert
            Assert.Equal("Добро пожаловать!", _homeVm.WelcomeMessage);
            Assert.Empty(_homeVm.UpcomingLessons);
            Assert.Equal(0, _homeVm.AbsencesCount);
            Assert.Equal("0 / 30", _homeVm.AbsencesDisplayText);
            Assert.Equal(0, _homeVm.SubjectsCount);
            Assert.Equal(0.0, _homeVm.AverageGradeValue);
            Assert.False(_homeVm.HasGradesToDisplay);
            Assert.Equal("Неизвестно", _homeVm.AcademicYear);
        }

        [Fact]
        public async Task LoadAllHomeData_WithUserButNoGroup_SetsDefaultValues()
        {
            // Arrange
            SetupTest();

            var userWithoutGroup = new User
            {
                UserID = 4001,
                FirstName = "Нет",
                LastName = "Группы",
                GroupID = null,
                RoleID = _studentRole.RoleID
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, userWithoutGroup);

            // Act
            _messenger.Send(new UserAuthenticatedMessage(userWithoutGroup));
            await Task.Delay(100);

            // Assert
            Assert.Equal("Добро пожаловать!", _homeVm.WelcomeMessage);
            Assert.Empty(_homeVm.UpcomingLessons);
            Assert.Equal(0, _homeVm.AbsencesCount);
            Assert.Equal("0 / 30", _homeVm.AbsencesDisplayText);
            Assert.Equal(0, _homeVm.SubjectsCount);
            Assert.Equal(0.0, _homeVm.AverageGradeValue);
            Assert.False(_homeVm.HasGradesToDisplay);
            Assert.Equal("Неизвестно", _homeVm.AcademicYear);
        }

        [Fact]
        public async Task LoadAnalyticsData_WithInvalidGrades_HandlesGracefully()
        {
            // Arrange
            SetupTest();

            // Remove ap1, or change its grade to an invalid one
            // var ap1 = new AcademicPerformance { PerformanceID = 50001, StudentID = _testUserStudent.UserID, Attendance = true, Grade = "5" }; // REMOVE THIS LINE
            var apInvalid = new AcademicPerformance { PerformanceID = 50001, StudentID = _testUserStudent.UserID, Attendance = true, Grade = "not-a-grade" }; // OR CHANGE TO INVALID
            var ap3 = new AcademicPerformance { PerformanceID = 50003, StudentID = _testUserStudent.UserID, Attendance = true, Grade = "abc" }; // Incorrect grade, attended

            // Seed only apInvalid and ap3 (or just ap3 if you only need one invalid)
            _testDbContextFactory.SeedData(_currentTestDbContext, apInvalid, ap3); // Or just ap3 if apInvalid is redundant

            _messenger.Send(new UserAuthenticatedMessage(_testUserStudent));
            await Task.Delay(100);

            Assert.False(_homeVm.HasGradesToDisplay); // Now this should be False
            Assert.Equal(0.0, _homeVm.AverageGradeValue);
            Assert.Equal("Н/Д", _homeVm.AverageGradeDisplayText);

            Assert.Equal(0, _homeVm.AbsencesCount);
            Assert.Equal("0 / 30", _homeVm.AbsencesDisplayText);
        }

        [Fact]
        public async Task LoadUpcomingLessonsInternal_WithNoGroup_DoesNotThrow()
        {
            // Arrange
            SetupTest();

            var userNoGroup = new User
            {
                UserID = 6001,
                FirstName = "Ни",
                LastName = "Группы",
                GroupID = null,
                RoleID = _studentRole.RoleID
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, userNoGroup);

            // Act
            _messenger.Send(new UserAuthenticatedMessage(userNoGroup));
            await Task.Delay(100);

            // Assert
            Assert.Empty(_homeVm.UpcomingLessons);
            Assert.Equal(0, _homeVm.AbsencesCount);
            Assert.False(_homeVm.HasGradesToDisplay);
            Assert.True(true);
        }
    }
}