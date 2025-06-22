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
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class LessonsVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext;
        private LessonsVm _lessonsVm;
        private IMessenger _messenger;

        private Role _studentRole;
        private Role _teacherRole;
        private User _testTeacher;
        private Group _group10;
        private Subject _subjectPhysics;
        private Classroom _classroom101;
        private StudyGroup _studyGroupPhysics10;

        public LessonsVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }
        private void SetupTest()
        {
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            _studentRole = new Role { RoleID = 10, RoleName = "Student" };
            _teacherRole = new Role { RoleID = 11, RoleName = "Teacher" };
            _group10 = new Group { GroupID = 10, GroupName = "Group 10" };
            _subjectPhysics = new Subject { SubjectID = 101, SubjectName = "Physics" };
            _classroom101 = new Classroom { ClassroomID = 1001, RoomNumber = "101" };

            _currentTestDbContext.Roles.Add(_studentRole);
            _currentTestDbContext.Roles.Add(_teacherRole);
            _currentTestDbContext.Groups.Add(_group10);
            _currentTestDbContext.Subjects.Add(_subjectPhysics);
            _currentTestDbContext.Classrooms.Add(_classroom101);

            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _testTeacher = new User
            {
                UserID = 100,
                LastName = "Иванов",
                FirstName = "Алексей",
                MiddleName = "Владимирович",
                RoleID = _teacherRole.RoleID,
            };

            _studyGroupPhysics10 = new StudyGroup
            {
                StudyGroupID = 10001,
                GroupID = _group10.GroupID,
                SubjectID = _subjectPhysics.SubjectID,
                TeacherID = _testTeacher.UserID,
            };

            _currentTestDbContext.Users.Add(_testTeacher);
            _currentTestDbContext.StudyGroups.Add(_studyGroupPhysics10);

            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _lessonsVm = new LessonsVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
        }

        [Fact]
        public async Task LoadAllStudentLessons_LoadsLessons_WhenUserHasGroupId()
        {
            SetupTest();

            var studentUser = new User
            {
                UserID = 200,
                GroupID = _group10.GroupID,
                RoleID = _studentRole.RoleID,
                FirstName = "Студент",
                LastName = "Тестовый"
            };
            await _currentTestDbContext.Users.AddAsync(studentUser);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            var lesson = new Lesson
            {
                LessonID = 1,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                ClassroomID = _classroom101.ClassroomID,
                StudyGroupID = _studyGroupPhysics10.StudyGroupID,
            };

            await _currentTestDbContext.Lessons.AddAsync(lesson);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100);

            Assert.Single(_lessonsVm.AllStudentLessons);
            var loadedLesson = _lessonsVm.AllStudentLessons.First();

            Assert.Equal(lesson.LessonID, loadedLesson.LessonId);
            Assert.Equal(_subjectPhysics.SubjectName, loadedLesson.SubjectName);
            Assert.Equal("Иванов А.В.", loadedLesson.TeacherFullName);
            Assert.Equal(_classroom101.RoomNumber, loadedLesson.RoomNumber);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserIsNull()
        {
            SetupTest();

            _lessonsVm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(_lessonsVm.AllStudentLessons);

            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.Empty(_lessonsVm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserGroupIdIsNull()
        {
            SetupTest();

            var userWithoutGroup = new User
            {
                UserID = 300,
                GroupID = null,
                RoleID = _studentRole.RoleID,
                FirstName = "НетГруппы",
                LastName = "Тест"
            };
            await _currentTestDbContext.Users.AddAsync(userWithoutGroup);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _lessonsVm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(_lessonsVm.AllStudentLessons);

            _messenger.Send(new UserAuthenticatedMessage(userWithoutGroup));
            await Task.Delay(100);

            Assert.Empty(_lessonsVm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_HandlesExceptionAndClearsLessons()
        {
            SetupTest();

            var mockFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            mockFactory.Setup(f => f.CreateDbContext()).Throws(new Exception("Simulated DB failure"));

            var vmWithMock = new LessonsVm(mockFactory.Object, _messenger);

            var studentUser = new User { UserID = 400, GroupID = _group10.GroupID };
            _testDbContextFactory.SeedData(_currentTestDbContext, studentUser);

            vmWithMock.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(vmWithMock.AllStudentLessons);

            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100);

            Assert.Empty(vmWithMock.AllStudentLessons);
        }

        [Fact]
        public async Task UserAuthenticatedMessage_TriggersLoadAllStudentLessons()
        {
            SetupTest();

            var studentUser = new User
            {
                UserID = 500,
                GroupID = _group10.GroupID,
                RoleID = _studentRole.RoleID,
                FirstName = "Студент",
                LastName = "Триггер"
            };
            await _currentTestDbContext.Users.AddAsync(studentUser);

            var lesson = new Lesson
            {
                LessonID = 2,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                ClassroomID = _classroom101.ClassroomID,
                StudyGroupID = _studyGroupPhysics10.StudyGroupID,
            };
            await _currentTestDbContext.Lessons.AddAsync(lesson);

            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100);

            var currentUser = (User)typeof(LessonsVm)
                .GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_lessonsVm);
            Assert.NotNull(currentUser);
            Assert.Equal(studentUser.GroupID, currentUser.GroupID);

            Assert.Single(_lessonsVm.AllStudentLessons);
            Assert.Equal(lesson.LessonID, _lessonsVm.AllStudentLessons.First().LessonId);
        }

        [Fact]
        public async Task TeacherFullNameFormat_WithoutMiddleName()
        {
            SetupTest();

            var teacherUserNoMiddleName = new User
            {
                UserID = 101,
                LastName = "Петров",
                FirstName = "Иван",
                MiddleName = null,
                RoleID = _teacherRole.RoleID,
            };
            await _currentTestDbContext.Users.AddAsync(teacherUserNoMiddleName);

            var subjectMath = new Subject { SubjectID = 102, SubjectName = "Math" };
            await _currentTestDbContext.Subjects.AddAsync(subjectMath);

            var studyGroupMathForPetrov = new StudyGroup
            {
                StudyGroupID = 10002,
                GroupID = _group10.GroupID,
                SubjectID = subjectMath.SubjectID,
                TeacherID = teacherUserNoMiddleName.UserID,
            };
            await _currentTestDbContext.StudyGroups.AddAsync(studyGroupMathForPetrov);

            var classroom202 = new Classroom { ClassroomID = 1002, RoomNumber = "202" };
            await _currentTestDbContext.Classrooms.AddAsync(classroom202);

            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            var lessonForPetrov = new Lesson
            {
                LessonID = 3,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                ClassroomID = classroom202.ClassroomID,
                StudyGroupID = studyGroupMathForPetrov.StudyGroupID,
            };
            await _currentTestDbContext.Lessons.AddAsync(lessonForPetrov);

            var studentUser = new User
            {
                UserID = 600,
                GroupID = _group10.GroupID,
                RoleID = _studentRole.RoleID,
            };
            await _currentTestDbContext.Users.AddAsync(studentUser);

            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100);

            Assert.Single(_lessonsVm.AllStudentLessons);
            var loadedLesson = _lessonsVm.AllStudentLessons.First();
            Assert.Equal("Петров И.", loadedLesson.TeacherFullName);
        }
    }
}