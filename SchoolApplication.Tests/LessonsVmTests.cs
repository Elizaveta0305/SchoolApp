using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic; // Добавлено
using System.Linq;
using System.Reflection; // Используется для _initialLoadCompletionSource, если поле приватное
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")] // Используем ту же коллекцию для мессенджера
    public class LessonsVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext; // Будет пересоздаваться для каждого теста
        private LessonsVm _lessonsVm;
        private IMessenger _messenger;

        // Общие тестовые данные, инициализируемые в SetupTest
        private Role _studentRole;
        private Role _teacherRole;
        private User _testTeacher;
        private Group _group10; // Группа для студента
        private Subject _subjectPhysics;
        private Classroom _classroom101;
        private StudyGroup _studyGroupPhysics10; // Учебная группа по физике для группы 10

        public LessonsVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }

        // Метод для настройки окружения перед каждым тестом
        private void SetupTest()
        {
            // Создаем новый DbContext для каждого теста
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            // Инициализируем базовые данные с уникальными ID
            _studentRole = new Role { RoleID = 10, RoleName = "Student" };
            _teacherRole = new Role { RoleID = 11, RoleName = "Teacher" };

            _testTeacher = new User
            {
                UserID = 100,
                LastName = "Иванов",
                FirstName = "Алексей",
                MiddleName = "Владимирович",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };

            _group10 = new Group { GroupID = 10, GroupName = "Group 10" };
            _subjectPhysics = new Subject { SubjectID = 101, SubjectName = "Physics" };
            _classroom101 = new Classroom { ClassroomID = 1001, RoomNumber = "101" };

            _studyGroupPhysics10 = new StudyGroup
            {
                StudyGroupID = 10001,
                GroupID = _group10.GroupID,
                SubjectID = _subjectPhysics.SubjectID,
                TeacherID = _testTeacher.UserID,
                Group = _group10,
                Subject = _subjectPhysics,
                Teacher = _testTeacher
            };

            // Посев всех общих базовых данных одним вызовом SeedData
            _testDbContextFactory.SeedData(
                _currentTestDbContext,
                _studentRole,
                _teacherRole,
                _testTeacher,
                _group10,
                _subjectPhysics,
                _classroom101,
                _studyGroupPhysics10
            );

            // Инициализируем ViewModel
            _lessonsVm = new LessonsVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            // Убеждаемся, что контекст БД высвобожден после каждого теста
            _currentTestDbContext?.Dispose();
        }

        [Fact]
        public async Task LoadAllStudentLessons_LoadsLessons_WhenUserHasGroupId()
        {
            // Arrange
            SetupTest();

            // Создаем студента, который будет аутентифицирован
            var studentUser = new User
            {
                UserID = 200,
                GroupID = _group10.GroupID, // Присваиваем ID существующей группы
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                FirstName = "Студент",
                LastName = "Тестовый"
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studentUser); // Добавляем студента в БД

            // Создаем урок для этой учебной группы
            var lesson = new Lesson
            {
                LessonID = 1,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                ClassroomID = _classroom101.ClassroomID,
                Classroom = _classroom101,
                StudyGroupID = _studyGroupPhysics10.StudyGroupID,
                StudyGroup = _studyGroupPhysics10
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lesson); // Добавляем урок

            // Act
            // Вместо использования Reflection, отправляем сообщение об аутентификации
            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100); // Даем время для асинхронной загрузки данных

            // Assert
            Assert.Single(_lessonsVm.AllStudentLessons);
            var loadedLesson = _lessonsVm.AllStudentLessons.First();

            Assert.Equal(lesson.LessonID, loadedLesson.LessonId);
            Assert.Equal(_subjectPhysics.SubjectName, loadedLesson.SubjectName);
            Assert.Equal("Иванов А.В.", loadedLesson.TeacherFullName); // Проверяем правильный формат
            Assert.Equal(_classroom101.RoomNumber, loadedLesson.RoomNumber);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserIsNull()
        {
            // Arrange
            SetupTest();

            // Предварительно добавляем какие-то уроки для очистки
            _lessonsVm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(_lessonsVm.AllStudentLessons);

            // Act
            // Отправляем сообщение с null пользователем, имитируя выход из системы
            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            // Assert
            Assert.Empty(_lessonsVm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserGroupIdIsNull()
        {
            // Arrange
            SetupTest();

            // Создаем пользователя без группы
            var userWithoutGroup = new User
            {
                UserID = 300,
                GroupID = null,
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                FirstName = "НетГруппы",
                LastName = "Тест"
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, userWithoutGroup);

            // Предварительно добавляем какие-то уроки
            _lessonsVm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(_lessonsVm.AllStudentLessons);

            // Act
            // Отправляем сообщение с пользователем без группы
            _messenger.Send(new UserAuthenticatedMessage(userWithoutGroup));
            await Task.Delay(100);

            // Assert
            Assert.Empty(_lessonsVm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_HandlesExceptionAndClearsLessons()
        {
            // Arrange
            SetupTest(); // Вызываем SetupTest для инициализации ViewModel и Messenger

            // Мокируем фабрику контекста так, чтобы CreateDbContext бросал исключение
            var mockFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            mockFactory.Setup(f => f.CreateDbContext()).Throws(new Exception("Simulated DB failure"));

            // Создаем LessonsVm с моком фабрики
            var vmWithMock = new LessonsVm(mockFactory.Object, _messenger);

            // Создаем студента для аутентификации
            var studentUser = new User { UserID = 400, GroupID = _group10.GroupID };
            _testDbContextFactory.SeedData(_currentTestDbContext, studentUser); // Добавляем студента в реальный контекст, если это нужно для других частей логики

            // Предварительно добавляем какие-то уроки для проверки очистки
            vmWithMock.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });
            Assert.NotEmpty(vmWithMock.AllStudentLessons);

            // Act
            // Отправляем сообщение, которое вызовет LoadAllStudentLessons, который теперь должен упасть
            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100); // Даем время для обработки исключения и очистки

            // Assert
            // Ожидаем, что коллекция уроков будет очищена из-за исключения
            Assert.Empty(vmWithMock.AllStudentLessons);
        }

        [Fact]
        public async Task UserAuthenticatedMessage_TriggersLoadAllStudentLessons()
        {
            // Arrange
            SetupTest();

            // Создаем студента
            var studentUser = new User
            {
                UserID = 500,
                GroupID = _group10.GroupID,
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                FirstName = "Студент",
                LastName = "Триггер"
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studentUser);

            // Создаем Lesson (необязательно, но для полноты)
            var lesson = new Lesson
            {
                LessonID = 2, // Уникальный ID
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                ClassroomID = _classroom101.ClassroomID,
                Classroom = _classroom101,
                StudyGroupID = _studyGroupPhysics10.StudyGroupID,
                StudyGroup = _studyGroupPhysics10
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lesson);

            // Используем TaskCompletionSource для ожидания завершения асинхронной операции
            // Если _initialLoadCompletionSource - это приватное поле, вам может потребоваться получить к нему доступ через Reflection,
            // как в вашем оригинальном тесте. Однако, если ViewModel имеет публичный метод или свойство, указывающее на завершение загрузки,
            // используйте его. Предполагая, что LoadAllStudentLessons вызывается при получении сообщения и заполняет коллекцию.
            // Более надежный способ - просто дождаться заполнения коллекции после отправки сообщения.

            // Act
            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100); // Даем время для асинхронной загрузки

            // Assert
            // Проверяем, что _currentUser был установлен
            var currentUser = (User)typeof(LessonsVm)
                .GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_lessonsVm);
            Assert.NotNull(currentUser);
            Assert.Equal(studentUser.GroupID, currentUser.GroupID);

            // Проверяем, что уроки загрузились
            Assert.Single(_lessonsVm.AllStudentLessons);
            Assert.Equal(lesson.LessonID, _lessonsVm.AllStudentLessons.First().LessonId);
        }

        [Fact]
        public async Task TeacherFullNameFormat_WithoutMiddleName()
        {
            // Arrange
            SetupTest();

            // Создаем учителя без отчества
            var teacherUserNoMiddleName = new User
            {
                UserID = 101, // Уникальный ID учителя
                LastName = "Петров",
                FirstName = "Иван",
                MiddleName = null, // Нет отчества
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, teacherUserNoMiddleName);

            // Создаем новый StudyGroup для этого учителя, чтобы его уроки не пересекались
            var subjectMath = new Subject { SubjectID = 102, SubjectName = "Math" };
            _testDbContextFactory.SeedData(_currentTestDbContext, subjectMath);

            var studyGroupMathForPetrov = new StudyGroup
            {
                StudyGroupID = 10002, // Уникальный ID StudyGroup
                GroupID = _group10.GroupID,
                SubjectID = subjectMath.SubjectID,
                TeacherID = teacherUserNoMiddleName.UserID,
                Group = _group10,
                Subject = subjectMath,
                Teacher = teacherUserNoMiddleName
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studyGroupMathForPetrov);

            var classroom202 = new Classroom { ClassroomID = 1002, RoomNumber = "202" };
            _testDbContextFactory.SeedData(_currentTestDbContext, classroom202);

            var lessonForPetrov = new Lesson
            {
                LessonID = 3, // Уникальный ID урока
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                ClassroomID = classroom202.ClassroomID,
                Classroom = classroom202,
                StudyGroupID = studyGroupMathForPetrov.StudyGroupID,
                StudyGroup = studyGroupMathForPetrov
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, lessonForPetrov);

            // Создаем студента, который будет аутентифицирован для этой группы
            var studentUser = new User
            {
                UserID = 600,
                GroupID = _group10.GroupID,
                RoleID = _studentRole.RoleID,
                Role = _studentRole
            };
            _testDbContextFactory.SeedData(_currentTestDbContext, studentUser);

            // Act
            _messenger.Send(new UserAuthenticatedMessage(studentUser));
            await Task.Delay(100);

            // Assert
            Assert.Single(_lessonsVm.AllStudentLessons);
            var loadedLesson = _lessonsVm.AllStudentLessons.First();
            Assert.Equal("Петров И.", loadedLesson.TeacherFullName); // Ожидаем "Фамилия И."
        }
    }
}