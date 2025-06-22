using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.Models.DisplayModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.AccessControl;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class LessonTeacherVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext;
        private LessonTeacherVm _lessonTeacherVm;
        private IMessenger _messenger;

        private Role _teacherRole;
        private Role _studentRole;

        private User _testTeacher;

        private Group _groupA;
        private Group _groupB;
        private Subject _subjectMath;
        private Subject _subjectHistory;
        private Classroom _classroom101;
        private StudyGroup _studyGroupMathA;
        private StudyGroup _studyGroupMathB;
        private StudyGroup _studyGroupHistoryA;

        public LessonTeacherVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }
        private void SetupTest()
        {
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            _teacherRole = new Role { RoleID = 62, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 74, RoleName = "Student" };

            _testTeacher = new User
            {
                UserID = 10,
                FirstName = "Иван",
                LastName = "Петров",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };

            _groupA = new Group { GroupID = 701, GroupName = "Group A" };
            _groupB = new Group { GroupID = 702, GroupName = "Group B" };
            _subjectMath = new Subject { SubjectID = 301, SubjectName = "Mathematics" };
            _subjectHistory = new Subject { SubjectID = 402, SubjectName = "История" };
            _classroom101 = new Classroom { ClassroomID = 601, RoomNumber = "101" };

            _studyGroupMathA = new StudyGroup
            {
                StudyGroupID = 5,
                TeacherID = _testTeacher.UserID,
                GroupID = _groupA.GroupID,
                SubjectID = _subjectMath.SubjectID,
                Teacher = _testTeacher,
                Group = _groupA,
                Subject = _subjectMath
            };
            _studyGroupMathB = new StudyGroup
            {
                StudyGroupID = 6,
                TeacherID = _testTeacher.UserID,
                GroupID = _groupB.GroupID,
                SubjectID = _subjectMath.SubjectID,
                Teacher = _testTeacher,
                Group = _groupB,
                Subject = _subjectMath
            };
            _studyGroupHistoryA = new StudyGroup
            {
                StudyGroupID = 7,
                TeacherID = _testTeacher.UserID,
                GroupID = _groupA.GroupID,
                SubjectID = _subjectHistory.SubjectID,
                Teacher = _testTeacher,
                Group = _groupA,
                Subject = _subjectHistory
            };

            _testDbContextFactory.SeedData(
                _currentTestDbContext,
                _teacherRole,
                _studentRole,
                _testTeacher,
                _groupA,
                _groupB,
                _subjectMath,
                _subjectHistory,
                _classroom101,
                _studyGroupMathA,
                _studyGroupMathB,
                _studyGroupHistoryA
            );

            _lessonTeacherVm = new LessonTeacherVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
        }

        [Fact]
        public void Constructor_InitializesCollectionsAndRegistersMessenger()
        {
            SetupTest();

            Assert.NotNull(_lessonTeacherVm.LessonsCollection);
            Assert.Empty(_lessonTeacherVm.LessonsCollection);

            Assert.NotNull(_lessonTeacherVm.Groups);
            Assert.Empty(_lessonTeacherVm.Groups);

            Assert.NotNull(_lessonTeacherVm.Subjects);
            Assert.Empty(_lessonTeacherVm.Subjects);

            Assert.NotNull(_lessonTeacherVm.Classrooms);
            Assert.Empty(_lessonTeacherVm.Classrooms);
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsInitialData()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));

            await Task.Delay(100);

            Assert.Equal(2, _lessonTeacherVm.Groups.Count);

            Assert.Contains(_lessonTeacherVm.Groups, g => g.GroupID == _groupA.GroupID && g.GroupName == _groupA.GroupName);
            Assert.Contains(_lessonTeacherVm.Groups, g => g.GroupID == _groupB.GroupID && g.GroupName == _groupB.GroupName);

            Assert.Equal(2, _lessonTeacherVm.Subjects.Count);
            Assert.Contains(_lessonTeacherVm.Subjects, s => s.SubjectID == _subjectMath.SubjectID && s.SubjectName == _subjectMath.SubjectName);
            Assert.Contains(_lessonTeacherVm.Subjects, s => s.SubjectID == _subjectHistory.SubjectID && s.SubjectName == _subjectHistory.SubjectName);

            Assert.Equal(1, _lessonTeacherVm.Classrooms.Count);
            Assert.Contains(_lessonTeacherVm.Classrooms, c => c.ClassroomID == _classroom101.ClassroomID && c.RoomNumber == _classroom101.RoomNumber);

            Assert.Empty(_lessonTeacherVm.LessonsCollection);
        }

        [Fact]
        public async Task Receive_NullUserAuthenticatedMessage_ClearsData()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            _lessonTeacherVm.LessonsCollection.Add(new LessonTeacherDisplayModel { LessonId = 999 });
            _lessonTeacherVm.Groups.Add(new Group { GroupID = 999, GroupName = "Temp" });

            Assert.NotEmpty(_lessonTeacherVm.LessonsCollection);
            Assert.NotEmpty(_lessonTeacherVm.Groups);

            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.Empty(_lessonTeacherVm.LessonsCollection);
            Assert.Empty(_lessonTeacherVm.Groups);
            Assert.Empty(_lessonTeacherVm.Subjects);
            Assert.Empty(_lessonTeacherVm.Classrooms);
            Assert.Null(_lessonTeacherVm.SelectedGroup);
            Assert.Null(_lessonTeacherVm.SelectedSubject);
            Assert.Null(_lessonTeacherVm.SelectedClassroom);
            Assert.Null(_lessonTeacherVm.LessonTopicInput);
        }

        [Fact]
        public async Task LoadLessonsDataCommand_LoadsLessonsForTeacher()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            var lesson = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Основы алгебры",
                ClassroomID = _classroom101.ClassroomID,
            };

            await _currentTestDbContext.Lessons.AddAsync(lesson);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await _lessonTeacherVm.LoadLessonsDataCommand.ExecuteAsync(null);

            Assert.Single(_lessonTeacherVm.LessonsCollection);
            var actualLesson = _lessonTeacherVm.LessonsCollection.First();

            Assert.Equal(lesson.Topic, actualLesson.Topic);
            Assert.Equal(_groupA.GroupName, actualLesson.GroupName);
            Assert.Equal(_subjectMath.SubjectName, actualLesson.SubjectName);
            Assert.Equal(_classroom101.RoomNumber, actualLesson.ClassroomNumber);
        }


        [Fact]
        public async Task LoadLessonsDataCommand_FiltersBySelectedGroup()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            var lessonA = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Занятие для Группы А",
                ClassroomID = _classroom101.ClassroomID,
            };
            var lessonB = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _studyGroupMathB.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Занятие для Группы Б",
                ClassroomID = _classroom101.ClassroomID,
            };

            await _currentTestDbContext.Lessons.AddRangeAsync(lessonA, lessonB);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _lessonTeacherVm.SelectedGroup = _groupA;

            await Task.Delay(100);

            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task LoadLessonsDataCommand_FiltersBySelectedSubject()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            var lessonMath = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Урок математики",
                ClassroomID = _classroom101.ClassroomID,
            };
            var lessonHistory = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _studyGroupHistoryA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Урок истории",
                ClassroomID = _classroom101.ClassroomID,
            };

            await _currentTestDbContext.Lessons.AddRangeAsync(lessonMath, lessonHistory);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _lessonTeacherVm.SelectedSubject = _subjectMath;

            await Task.Delay(100);

            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedGroupChanged_TriggersLoadLessonsDataAsync()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            var lessonForGroupA = new Lesson
            {
                LessonID = 100,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(11, 0, 0),
                Topic = "Test Topic for Group A",
                ClassroomID = _classroom101.ClassroomID,
            };

            await _currentTestDbContext.Lessons.AddAsync(lessonForGroupA);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            Assert.Empty(_lessonTeacherVm.LessonsCollection);

            _lessonTeacherVm.SelectedGroup = _groupA;
            await Task.Delay(100);

            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForGroupA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_TriggersLoadLessonsDataAsync()
        {
            SetupTest();

            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            var lessonForSubjectMath = new Lesson
            {
                LessonID = 101,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(12, 0, 0),
                Topic = "Test Topic for Math Subject",
                ClassroomID = _classroom101.ClassroomID,
            };

            await _currentTestDbContext.Lessons.AddAsync(lessonForSubjectMath);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            Assert.Empty(_lessonTeacherVm.LessonsCollection);

            _lessonTeacherVm.SelectedSubject = _subjectMath;
            await Task.Delay(100);

            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForSubjectMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }
    }
}