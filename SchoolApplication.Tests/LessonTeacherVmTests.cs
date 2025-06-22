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
        private ApplicationDbContext _currentTestDbContext; // Будет пересоздаваться для каждого теста
        private LessonTeacherVm _lessonTeacherVm;
        private IMessenger _messenger;

        // Вспомогательные роли для тестирования (пересоздаются в SetupTest)
        private Role _teacherRole;
        private Role _studentRole;

        // Вспомогательный пользователь для тестирования (пересоздается в SetupTest)
        private User _testTeacher;

        // Вспомогательные данные (пересоздаются в SetupTest)
        private Group _groupA;
        private Group _groupB; // Добавлено для теста фильтрации
        private Subject _subjectMath;
        private Subject _subjectHistory; // Добавлено для теста фильтрации
        private Classroom _classroom101;
        private StudyGroup _studyGroupMathA;
        private StudyGroup _studyGroupMathB; // Добавлено для теста фильтрации
        private StudyGroup _studyGroupHistoryA; // Добавлено для теста фильтрации

        public LessonTeacherVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }

        // Метод для настройки окружения перед каждым тестом
        private void SetupTest()
        {
            // Получаем НОВЫЙ DbContext из фабрики для КАЖДОГО ТЕСТА
            // Этот контекст будет иметь уникальное имя базы данных благодаря TestDbContextFactory.
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            // Настраиваем общие тестовые данные - свежие данные для каждого теста
            // Эти вызовы SeedData теперь будут добавлять в *пустую* базу данных каждый раз, когда вызывается SetupTest,
            // потому что CreateDbContext обеспечивает уникальное имя базы данных в памяти.
            _teacherRole = new Role { RoleID = 62, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 74, RoleName = "Student" };
            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole);
            _currentTestDbContext.ChangeTracker.Clear();

            _testTeacher = new User
            {
                UserID = 10,
                FirstName = "Иван",
                LastName = "Петров",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, _testTeacher);
            _currentTestDbContext.ChangeTracker.Clear();

            _groupA = new Group { GroupID = 701, GroupName = "Group A" };
            _groupB = new Group { GroupID = 702, GroupName = "Group B" };
            _subjectMath = new Subject { SubjectID = 301, SubjectName = "Mathematics" };
            _subjectHistory = new Subject { SubjectID = 402, SubjectName = "История" };
            _classroom101 = new Classroom { ClassroomID = 601, RoomNumber = "101" };
            _testDbContextFactory.SeedData(_currentTestDbContext, _groupA, _groupB, _subjectMath, _subjectHistory, _classroom101);
            _currentTestDbContext.ChangeTracker.Clear();

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
                _teacherRole, _studentRole,
                _testTeacher,
                _groupA, _groupB, _subjectMath, _subjectHistory, _classroom101,
                _studyGroupMathA, _studyGroupMathB, _studyGroupHistoryA
            );
            _lessonTeacherVm = new LessonTeacherVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            // Убедитесь, что текущий тестовый контекст высвобожден.
            _currentTestDbContext?.Dispose();
        }

        [Fact]
        public void Constructor_InitializesCollectionsAndRegistersMessenger()
        {
            // Arrange
            SetupTest();

            // Assert
            Assert.NotNull(_lessonTeacherVm.LessonsCollection);
            Assert.Empty(_lessonTeacherVm.LessonsCollection); // Должно быть пустым до аутентификации

            Assert.NotNull(_lessonTeacherVm.Groups);
            Assert.Empty(_lessonTeacherVm.Groups); // Должно быть пустым до аутентификации

            Assert.NotNull(_lessonTeacherVm.Subjects);
            Assert.Empty(_lessonTeacherVm.Subjects); // Должно быть пустым до аутентификации

            Assert.NotNull(_lessonTeacherVm.Classrooms);
            Assert.Empty(_lessonTeacherVm.Classrooms); // Должно быть пустым до аутентификации
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsInitialData()
        {
            // Arrange
            SetupTest();

            // Act
            // Отправляем сообщение об аутентификации пользователя
            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));

            // Небольшая задержка, чтобы асинхронные операции успели выполниться, так как Receive - это async void
            await Task.Delay(100);

            // Assert
            // Теперь данные должны быть загружены на основе _testTeacher
            Assert.Equal(1, _lessonTeacherVm.Groups.Count);
            Assert.Contains(_groupA, _lessonTeacherVm.Groups);

            Assert.Equal(1, _lessonTeacherVm.Subjects.Count);
            Assert.Contains(_subjectMath, _lessonTeacherVm.Subjects);

            Assert.Equal(1, _lessonTeacherVm.Classrooms.Count);
            Assert.Contains(_classroom101, _lessonTeacherVm.Classrooms);

            // Изначально уроков нет, поэтому коллекция должна быть пустой
            Assert.Empty(_lessonTeacherVm.LessonsCollection);
        }

        [Fact]
        public async Task Receive_NullUserAuthenticatedMessage_ClearsData()
        {
            // Arrange
            SetupTest();

            // Сначала аутентифицируем пользователя для заполнения данных
            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Даем данным загрузиться

            // Добавляем некоторые фиктивные данные в коллекцию уроков для проверки очистки
            _lessonTeacherVm.LessonsCollection.Add(new LessonTeacherDisplayModel { LessonId = 999 });
            _lessonTeacherVm.Groups.Add(new Group { GroupID = 999, GroupName = "Temp" });

            // Проверяем предварительное условие
            Assert.NotEmpty(_lessonTeacherVm.LessonsCollection);
            Assert.NotEmpty(_lessonTeacherVm.Groups);

            // Act
            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100); // Даем асинхронным операциям выполниться

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
            SetupTest();

            // Аутентифицируем пользователя, чтобы ViewModel мог загрузить данные
            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Даем initial data load завершиться

            // Создаем урок, который принадлежит этому учителю через _studyGroupMathA
            var lesson = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _studyGroupMathA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Основы алгебры",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathA, // Явно связываем для теста
                Classroom = _classroom101 // Явно связываем для теста
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lesson);
            // ChangeTracker.Clear() уже происходит внутри SeedData, но здесь это не обязательно
            // так как мы не будем делать SaveChanges в этом контексте после SeedData.

            // Act
            // Выполняем команду LoadLessonsDataCommand
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
            SetupTest();

            // Аутентифицируем пользователя
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
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            var lessonB = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _studyGroupMathB.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Занятие для Группы Б",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupMathB,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonA, lessonB);

            // Теперь ViewModel имеет доступ ко всем группам, но мы хотим фильтровать по Group A
            _lessonTeacherVm.SelectedGroup = _groupA; // Это автоматически вызовет LoadLessonsDataAsync

            // Act (уже вызвано установкой SelectedGroup)
            await Task.Delay(100); // Позволяем асинхронной операции завершиться

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task LoadLessonsDataCommand_FiltersBySelectedSubject()
        {
            // Arrange
            SetupTest();

            // Аутентифицируем пользователя
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
                StudyGroup = _studyGroupMathA,
                Classroom = _classroom101
            };
            var lessonHistory = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _studyGroupHistoryA.StudyGroupID,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Урок истории",
                ClassroomID = _classroom101.ClassroomID,
                StudyGroup = _studyGroupHistoryA,
                Classroom = _classroom101
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonMath, lessonHistory);

            // Теперь ViewModel имеет доступ ко всем предметам, но мы хотим фильтровать по Mathematics
            _lessonTeacherVm.SelectedSubject = _subjectMath; // Это автоматически вызовет LoadLessonsDataAsync

            // Act (уже вызвано установкой SelectedSubject)
            await Task.Delay(100); // Позволяем асинхронной операции завершиться

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedGroupChanged_TriggersLoadLessonsDataAsync()
        {
            // Arrange
            SetupTest();

            // Аутентифицируем пользователя, чтобы ViewModel мог загрузить данные и реагировать на изменения
            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100); // Даем initial data load завершиться

            // Добавляем урок, который должен быть отфильтрован
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
            // ViewModel должен быть пуст, так как LoadLessonsDataAsync еще не вызван после добавления урока
            Assert.Empty(_lessonTeacherVm.LessonsCollection);

            // Act
            // Изменяем выбранную группу. Это должно вызвать LoadLessonsDataAsync.
            _lessonTeacherVm.SelectedGroup = _groupA;
            await Task.Delay(100); // Даем асинхронной операции завершиться

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForGroupA.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_TriggersLoadLessonsDataAsync()
        {
            // Arrange
            SetupTest();

            // Аутентифицируем пользователя
            _messenger.Send(new UserAuthenticatedMessage(_testTeacher));
            await Task.Delay(100);

            // Добавляем урок, который должен быть отфильтрован
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
            // ViewModel должен быть пуст
            Assert.Empty(_lessonTeacherVm.LessonsCollection);

            // Act
            // Изменяем выбранный предмет. Это должно вызвать LoadLessonsDataAsync.
            _lessonTeacherVm.SelectedSubject = _subjectMath;
            await Task.Delay(100); // Даем асинхронной операции завершиться

            // Assert
            Assert.Single(_lessonTeacherVm.LessonsCollection);
            Assert.Equal(lessonForSubjectMath.Topic, _lessonTeacherVm.LessonsCollection.First().Topic);
        }
    }
}