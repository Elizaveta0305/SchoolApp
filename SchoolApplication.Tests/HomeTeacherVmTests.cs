using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    public class HomeTeacherVmTests : IDisposable
    {
        private readonly TestDbContextFactory _testDbContextFactory;
        private readonly Mock<IMessenger> _mockMessenger;
        private HomeTeacherVm _viewModel;

        private User _teacherUser;
        private User _student1, _student2;
        private Group _group1;
        private Subject _math, _physics;
        private StudyGroup _mathStudyGroup, _physicsStudyGroup;
        private Classroom _room101;
        private Lesson _upcomingLessonToday, _pastLessonToday, _futureLessonTomorrow;
        private AcademicPerformance _grade1, _grade2;

        public HomeTeacherVmTests()
        {
            _testDbContextFactory = new TestDbContextFactory();
            _mockMessenger = new Mock<IMessenger>();

            _viewModel = new HomeTeacherVm(_testDbContextFactory, _mockMessenger.Object);

            InitializeTestData();
            SeedDatabase();
        }

        private void InitializeTestData()
        {
            _teacherUser = new User { UserID = 1, FirstName = "Иван", LastName = "Петров", MiddleName = "Иванович", RoleID = 2, Username = "teacher1", PasswordHash = "hash" };
            _group1 = new Group { GroupID = 1, GroupName = "10А" };
            _student1 = new User { UserID = 101, FirstName = "Мария", LastName = "Иванова", RoleID = 3, GroupID = _group1.GroupID };
            _student2 = new User { UserID = 102, FirstName = "Алексей", LastName = "Сидоров", RoleID = 3, GroupID = _group1.GroupID };
            _math = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _physics = new Subject { SubjectID = 2, SubjectName = "Физика" };
            _room101 = new Classroom { ClassroomID = 1, RoomNumber = "101" };

            _mathStudyGroup = new StudyGroup { StudyGroupID = 1, TeacherID = _teacherUser.UserID, GroupID = _group1.GroupID, SubjectID = _math.SubjectID };
            _physicsStudyGroup = new StudyGroup { StudyGroupID = 2, TeacherID = _teacherUser.UserID, GroupID = _group1.GroupID, SubjectID = _physics.SubjectID };

            _pastLessonToday = new Lesson
            {
                LessonID = 10,
                StudyGroupID = _mathStudyGroup.StudyGroupID,
                ClassroomID = _room101.ClassroomID,
                Topic = "Прошедшая Тема",
                LessonDate = new DateTime(2025, 5, 10).Date,
                LessonTime = new TimeSpan(10, 0, 0)
            };
            _upcomingLessonToday = new Lesson
            {
                LessonID = 11,
                StudyGroupID = _mathStudyGroup.StudyGroupID,
                ClassroomID = _room101.ClassroomID,
                Topic = "Предстоящая Тема (скоро)",
                LessonDate = new DateTime(2025, 6, 23).Date,
                LessonTime = new TimeSpan(9, 0, 0)
            };
            _futureLessonTomorrow = new Lesson
            {
                LessonID = 12,
                StudyGroupID = _physicsStudyGroup.StudyGroupID,
                ClassroomID = _room101.ClassroomID,
                Topic = "Предстоящая Тема (далеко)",
                LessonDate = new DateTime(2025, 7, 1).Date,
                LessonTime = new TimeSpan(14, 0, 0)
            };

            _grade1 = new AcademicPerformance { PerformanceID = 1, StudentID = _student1.UserID, LessonID = _pastLessonToday.LessonID, Grade = "5", Attendance = true };
            _grade2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student2.UserID, LessonID = _pastLessonToday.LessonID, Grade = "4", Attendance = true };
        }

        private void SeedDatabase()
        {
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                dbContext.Users.Add(_teacherUser);
                dbContext.Groups.Add(_group1);
                dbContext.Users.AddRange(_student1, _student2);
                dbContext.Subjects.AddRange(_math, _physics);
                dbContext.Classrooms.Add(_room101);
                dbContext.StudyGroups.AddRange(_mathStudyGroup, _physicsStudyGroup);
                dbContext.Lessons.AddRange(_upcomingLessonToday, _pastLessonToday, _futureLessonTomorrow);
                dbContext.AcademicPerformance.AddRange(_grade1, _grade2);

                dbContext.SaveChanges();
            }
        }

        public void Dispose()
        {
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                dbContext.Database.EnsureDeleted();
            }
        }

        private async Task SetAuthenticatedUser(User user)
        {
            _viewModel.Receive(new UserAuthenticatedMessage(user));
            await Task.Delay(100);
        }

        [Fact]
        public async Task ReceiveUserAuthenticatedMessage_ValidUser_LoadsAllData()
        {
            await SetAuthenticatedUser(_teacherUser);

            Assert.Equal($"{_teacherUser.FirstName} {_teacherUser.MiddleName}", _viewModel.CurrentTeacherFullName);

            Assert.NotEmpty(_viewModel.UpcomingLessons);
            Assert.Contains(_viewModel.UpcomingLessons, l => l.LessonId == _upcomingLessonToday.LessonID);
            Assert.Contains(_viewModel.UpcomingLessons, l => l.LessonId == _futureLessonTomorrow.LessonID);
            Assert.DoesNotContain(_viewModel.UpcomingLessons, l => l.LessonId == _pastLessonToday.LessonID); // Прошедший урок не должен быть в "предстоящих"
            Assert.Equal(2, _viewModel.UpcomingLessons.Count);

            Assert.Equal(2, _viewModel.CurrentStudentCount);

            Assert.Equal(4.5, _viewModel.AverageGradeValue, 1);

            Assert.Equal(1, _viewModel.ConductedLessonsCount);
            Assert.Equal(3, _viewModel.TotalLessonsInAcademicYear);
            Assert.Contains("1 из 3 (33%)", _viewModel.ConductedLessonsDisplayText);
        }

        [Fact]
        public async Task ReceiveUserAuthenticatedMessage_NullUser_ClearsAllData()
        {
            await SetAuthenticatedUser(_teacherUser);
            Assert.NotNull(_viewModel.CurrentTeacherFullName);

            // Act
            _viewModel.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            // Assert
            Assert.Equal("Неизвестный", _viewModel.CurrentTeacherFullName);
            Assert.Empty(_viewModel.UpcomingLessons);
            Assert.Equal(0, _viewModel.CurrentStudentCount);
            Assert.Equal(0, _viewModel.AverageGradeValue);
            Assert.Equal(0, _viewModel.ConductedLessonsCount);
            Assert.Equal(0, _viewModel.TotalLessonsInAcademicYear);
            Assert.Contains("0 занятий", _viewModel.ConductedLessonsDisplayText);
        }

        [Fact]
        public async Task LoadAllTeacherHomeData_NoGrades_AverageGradeIsZero()
        {
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var gradesToDelete = await dbContext.AcademicPerformance
                    .Where(ap => ap.Lesson != null && ap.Lesson.StudyGroup != null && ap.Lesson.StudyGroup.TeacherID == _teacherUser.UserID)
                    .ToListAsync();
                dbContext.AcademicPerformance.RemoveRange(gradesToDelete);
                await dbContext.SaveChangesAsync();
            }

            await SetAuthenticatedUser(_teacherUser);

            Assert.Equal(0, _viewModel.AverageGradeValue);
            Assert.Contains("0.00", _viewModel.AverageGradeDisplayText);
        }

        [Fact]
        public async Task LoadAllTeacherHomeData_NoStudents_CurrentStudentCountIsZero()
        {
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var studentsToDelete = await dbContext.Users
                    .Where(u => u.RoleID == 3 && u.GroupID == _group1.GroupID)
                    .ToListAsync();
                dbContext.Users.RemoveRange(studentsToDelete);
                await dbContext.SaveChangesAsync();
            }

            await SetAuthenticatedUser(_teacherUser);

            Assert.Equal(0, _viewModel.CurrentStudentCount);
        }
    }
}