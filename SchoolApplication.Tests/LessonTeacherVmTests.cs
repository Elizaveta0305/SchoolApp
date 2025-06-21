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

namespace SchoolApplication.Tests
{
    public class LessonTeacherVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory; // Использование вашей реальной фабрики
        private ApplicationDbContext _currentTestDbContext; // Контекст, используемый для заполнения данных в текущем тесте
        private LessonTeacherVm _lessonTeacherVm;

        // Helper roles for testing
        private Role _teacherRole;
        private Role _studentRole;

        // Helper user for testing
        private User _testTeacher;

        // Helper data
        private Group _groupA;
        private Subject _subjectMath;
        private Classroom _classroom101;
        private StudyGroup _studyGroupMathA;

        public LessonTeacherVmTests()
        {
            // Инициализируем TestDbContextFactory без имени базы данных,
            // так как она теперь генерируется внутри CreateDbContext().
            _testDbContextFactory = new TestDbContextFactory();
        }

        // Метод для настройки окружения перед каждым тестом
        private void SetupTest()
        {
            // Получаем НОВЫЙ DbContext из фабрики для КАЖДОГО ТЕСТА
            // Этот контекст будет иметь уникальное имя базы данных.
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            // Setup common test data - fresh data for each test
            _teacherRole = new Role { RoleID = 1, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 2, RoleName = "Student" };
            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole);
            _currentTestDbContext.ChangeTracker.Clear();

            _testTeacher = new User
            {
                UserID = 1,
                FirstName = "Иван",
                LastName = "Петров",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, _testTeacher);
            _currentTestDbContext.ChangeTracker.Clear();

            _groupA = new Group { GroupID = 101, GroupName = "Group A" };
            _subjectMath = new Subject { SubjectID = 201, SubjectName = "Mathematics" };
            _classroom101 = new Classroom { ClassroomID = 301, RoomNumber = "101" };
            _testDbContextFactory.SeedData(_currentTestDbContext, _groupA, _subjectMath, _classroom101);
            _currentTestDbContext.ChangeTracker.Clear();

            _studyGroupMathA = new StudyGroup
            {
                StudyGroupID = 1,
                TeacherID = _testTeacher.UserID,
                GroupID = _groupA.GroupID,
                SubjectID = _subjectMath.SubjectID,
                Teacher = _testTeacher,
                Group = _groupA,
                Subject = _subjectMath
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, _studyGroupMathA);
            _currentTestDbContext.ChangeTracker.Clear();

            // Инициализируем ViewModel с нашей реальной фабрикой (TestDbContextFactory)
            _lessonTeacherVm = new LessonTeacherVm(_testDbContextFactory);

            // Reset messenger for each test
            WeakReferenceMessenger.Default.Reset();
        }

        public void Dispose()
        {
            // Убеждаемся, что текущий контекст теста диспоузится.
            // Каждый тест получит новый контекст, поэтому этот Dispose()
            // в основном очищает контекст, который был активен в последнем тесте.
            _currentTestDbContext?.Dispose();
            WeakReferenceMessenger.Default.Reset();
        }

        [Fact]
        public void Constructor_InitializesCollectionsAndRegistersMessenger()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            // Assert
            Assert.NotNull(_lessonTeacherVm.LessonsCollection);
            Assert.Empty(_lessonTeacherVm.LessonsCollection); // Should be empty before authentication

            Assert.NotNull(_lessonTeacherVm.Groups);
            Assert.Empty(_lessonTeacherVm.Groups); // Should be empty before authentication

            Assert.NotNull(_lessonTeacherVm.Subjects);
            Assert.Empty(_lessonTeacherVm.Subjects); // Should be empty before authentication

            Assert.NotNull(_lessonTeacherVm.Classrooms);
            Assert.Empty(_lessonTeacherVm.Classrooms); // Should be empty before authentication
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsInitialData()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            // Act
            _lessonTeacherVm.Receive(new UserAuthenticatedMessage(_testTeacher));

            // Small delay to allow async operations to propagate, as Receive is async void
            await Task.Delay(100);

            // Assert
            Assert.Equal(1, _lessonTeacherVm.Groups.Count);
            Assert.Contains(_groupA, _lessonTeacherVm.Groups);

            Assert.Equal(1, _lessonTeacherVm.Subjects.Count);
            Assert.Contains(_subjectMath, _lessonTeacherVm.Subjects);

            Assert.Equal(1, _lessonTeacherVm.Classrooms.Count);
            Assert.Contains(_classroom101, _lessonTeacherVm.Classrooms);

            // Initially, there are no lessons added, so collection should still be empty
            Assert.Empty(_lessonTeacherVm.LessonsCollection);
        }

        [Fact]
        public async Task Receive_NullUserAuthenticatedMessage_ClearsData()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            // First, authenticate a user to populate data
            _lessonTeacherVm.Receive(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Allow data to load

            // Add some dummy data to lessons collection for clearing test
            _lessonTeacherVm.LessonsCollection.Add(new LessonTeacherDisplayModel { LessonId = 999 });
            _lessonTeacherVm.Groups.Add(new Group { GroupID = 999, GroupName = "Temp" });

            // Assert pre-condition
            Assert.NotEmpty(_lessonTeacherVm.LessonsCollection);
            Assert.NotEmpty(_lessonTeacherVm.Groups);

            // Act
            _lessonTeacherVm.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(100); // Allow async operations to propagate

            // Assert
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
            // Arrange
            SetupTest(); // Call setup for this test

            var lesson = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Основы алгебры",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            // Добавляем урок через TestDbContextFactory.SeedData
            _testDbContextFactory.SeedData(_currentTestDbContext, lesson);
            // _currentTestDbContext.ChangeTracker.Clear(); // SeedData уже делает это

            // Act
            await _lessonTeacherVm.LoadLessonsDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lesson.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
            Assert.Equal(lesson.StudyGroup.Group.GroupName, _lessonTeacherVm.LessonsCollection.First().GroupName);
            Assert.Equal(lesson.StudyGroup.Subject.SubjectName, _lessonTeacherVm.LessonsCollection.First().SubjectName);
        }

        [Fact]
        public async Task LoadLessonsDataCommand_FiltersBySelectedGroup()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            var groupB = new Group { GroupID = 102, GroupName = "Group B" };
            _testDbContextFactory.SeedData(_currentTestDbContext, groupB);

            var studyGroupMathB = new StudyGroup
            {
                StudyGroupID = 2,
                TeacherID = _testTeacher.UserID,
                GroupID = groupB.GroupID,
                SubjectID = _subjectMath.SubjectID,
                Teacher = _testTeacher,
                Group = groupB,
                Subject = _subjectMath
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studyGroupMathB);


            var lessonA = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Занятие для Группы А",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            var lessonB = new Lesson
            {
                LessonID = 2,
                StudyGroupID = studyGroupMathB.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Занятие для Группы Б",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = studyGroupMathB,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonA, lessonB);
            // _currentTestDbContext.ChangeTracker.Clear(); // SeedData уже делает это

            _lessonTeacherVm.SelectedGroup = _groupA; // Set filter, this will trigger LoadLessonsDataAsync implicitly

            // Act (already triggered by setting SelectedGroup)
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task LoadLessonsDataCommand_FiltersBySelectedSubject()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            var subjectHistory = new Subject { SubjectID = 202, SubjectName = "История" };
            _testDbContextFactory.SeedData(_currentTestDbContext, subjectHistory);

            var studyGroupHistoryA = new StudyGroup
            {
                StudyGroupID = 3,
                TeacherID = _testTeacher.UserID,
                GroupID = _groupA.GroupID,
                SubjectID = subjectHistory.SubjectID,
                Teacher = _testTeacher,
                Group = _groupA,
                Subject = subjectHistory
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studyGroupHistoryA);

            var lessonMath = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Урок математики",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            var lessonHistory = new Lesson
            {
                LessonID = 2,
                StudyGroupID = studyGroupHistoryA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Урок истории",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = studyGroupHistoryA,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonMath, lessonHistory);
            // _currentTestDbContext.ChangeTracker.Clear(); // SeedData уже делает это

            _lessonTeacherVm.SelectedSubject = _subjectMath; // Set filter, this will trigger LoadLessonsDataAsync implicitly

            // Act (already triggered by setting SelectedSubject)
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedGroupChanged_TriggersLoadLessonsDataAsync()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            _lessonTeacherVm.Receive(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Allow initial data load

            // To make it pass meaningfully, let's ensure some lesson is there
            var lessonForGroupA = new Lesson
            {
                LessonID = 100,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(11, 0, 0),
                Topic = "Test Topic for Group A",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonForGroupA);
            // _currentTestDbContext.ChangeTracker.Clear(); // SeedData уже делает это

            // Act
            // Re-trigger the load after adding the lesson
            _lessonTeacherVm.SelectedGroup = _groupA;
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForGroupA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_TriggersLoadLessonsDataAsync()
        {
            // Arrange
            SetupTest(); // Call setup for this test

            _lessonTeacherVm.Receive(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Allow initial data load

            // Similar to SelectedGroup, let's make this test more robust by ensuring a lesson exists
            var lessonForSubjectMath = new Lesson
            {
                LessonID = 101,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(12, 0, 0),
                Topic = "Test Topic for Math Subject",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonForSubjectMath);
            // _currentTestDbContext.ChangeTracker.Clear(); // SeedData уже делает это

            // Act
            // Re-trigger the load after adding the lesson
            _lessonTeacherVm.SelectedSubject = _subjectMath;
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForSubjectMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }
    }
}