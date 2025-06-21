using Xunit;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SchoolApplication.Models.DisplayModels; // Убедитесь, что это пространство имен существует
using System;

namespace SchoolApplication.Tests
{
    // Реализуем IDisposable для очистки ресурсов после каждого теста
    public class GradeVmTests : IDisposable
    {
        private readonly TestDbContextFactory _dbContextFactory;
        private ApplicationDbContext _currentTestDbContext; // Контекст для текущего теста
        private GradeVm _viewModel;

        // Эти поля будут инициализированы в SetupTest() для каждого теста
        private Role _studentRole;
        private Role _teacherRole;
        private User _studentUser;
        private User _teacherUser;
        private Group _group9B;
        private Subject _stm32Subject;
        private Subject _scratchSubject;
        private StudyGroup _stm32StudyGroup;
        private StudyGroup _scratchStudyGroup;
        private Lesson _stm32Lesson1;
        private Lesson _scratchLesson1;
        private AcademicPerformance _studentStm32Grade;
        private AcademicPerformance _studentScratchGrade;

        public GradeVmTests()
        {
            // Фабрика теперь создается без аргументов, как мы её изменили.
            // Она будет генерировать уникальное имя базы данных для каждого CreateDbContext()
            _dbContextFactory = new TestDbContextFactory();
        }

        // Метод, который будет вызываться перед каждым тестом для инициализации
        private void SetupTest()
        {
            // Сброс Messenger для каждого теста, чтобы избежать влияния предыдущих тестов
            WeakReferenceMessenger.Default.Reset();

            // Создаем новый, чистый DbContext для каждого теста
            _currentTestDbContext = _dbContextFactory.CreateDbContext();

            // Инициализируем тестовые данные для каждого теста
            _studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            _teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            _group9B = new Group { GroupID = 201, GroupName = "9Б" }; // Убираем инициализацию коллекций здесь, EF Core сам их добавит

            _studentUser = new User
            {
                UserID = 101,
                Username = "student1",
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                RoleID = _studentRole.RoleID,
                GroupID = _group9B.GroupID,
                Role = _studentRole, // Добавляем ссылку на навигационное свойство для корректного связывания
                Group = _group9B
            };

            _teacherUser = new User { UserID = 102, Username = "teacher1", FirstName = "Иван", LastName = "Иванов", MiddleName = "Иванович", RoleID = _teacherRole.RoleID, Role = _teacherRole };

            _stm32Subject = new Subject { SubjectID = 301, SubjectName = "STM32 в среде STM32CubeIDE" };
            _scratchSubject = new Subject { SubjectID = 302, SubjectName = "Scratch" };

            _stm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 401,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _stm32Subject.SubjectID,
                Teacher = _teacherUser, // Навигационные свойства
                Group = _group9B,
                Subject = _stm32Subject
            };
            _scratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 402,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _scratchSubject.SubjectID,
                Teacher = _teacherUser,
                Group = _group9B,
                Subject = _scratchSubject
            };

            _stm32Lesson1 = new Lesson
            {
                LessonID = 501,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                LessonDate = new DateTime(2024, 05, 10),
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Введение в STM32CubeIDE",
                StudyGroup = _stm32StudyGroup // Навигационные свойства
            };
            _scratchLesson1 = new Lesson
            {
                LessonID = 502,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2024, 05, 11),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Создание первого проекта",
                StudyGroup = _scratchStudyGroup
            };

            _studentStm32Grade = new AcademicPerformance
            {
                PerformanceID = 601,
                StudentID = _studentUser.UserID,
                LessonID = _stm32Lesson1.LessonID,
                Grade = "5",
                Attendance = true,
                Comment = "Отличная работа с платой",
                Student = _studentUser, // Навигационные свойства
                Lesson = _stm32Lesson1
            };
            _studentScratchGrade = new AcademicPerformance
            {
                PerformanceID = 602,
                StudentID = _studentUser.UserID,
                LessonID = _scratchLesson1.LessonID,
                Grade = "4",
                Attendance = true,
                Comment = "Креативный подход",
                Student = _studentUser,
                Lesson = _scratchLesson1
            };

            // Сеем все базовые данные для большинства тестов
            SeedDatabase(
                _studentRole,
                _teacherRole,
                _group9B,
                _studentUser,
                _teacherUser,
                _stm32Subject,
                _scratchSubject,
                _stm32StudyGroup,
                _scratchStudyGroup,
                _stm32Lesson1,
                _scratchLesson1,
                _studentStm32Grade,
                _studentScratchGrade
            );

            // Инициализируем ViewModel с новой фабрикой для каждого теста
            _viewModel = new GradeVm(_dbContextFactory);
        }

        // Метод Dispose для очистки ресурсов после каждого теста
        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
            // Сброс Messenger после каждого теста
            WeakReferenceMessenger.Default.Reset();
        }

        // Вспомогательный метод для посева данных, использующий текущий DbContext
        private void SeedDatabase(params object[] entities)
        {
            _dbContextFactory.SeedData(_currentTestDbContext, entities);
            _currentTestDbContext.ChangeTracker.Clear(); // Отсоединяем сущности, чтобы избежать проблем с отслеживанием
        }

        // Вспомогательный метод для создания ViewModel и отправки сообщения аутентификации
        private async Task<GradeVm> CreateAndAuthenticateViewModel(User? currentUser = null)
        {
            // Здесь мы создаем ViewModel. Если currentUser не null, мы его аутентифицируем.
            // SetupTest уже создал _viewModel, так что просто используем его.
            if (currentUser != null)
            {
                User userFromDb;
                // Получаем пользователя с необходимыми включенными навигационными свойствами
                // из свежего контекста, чтобы избежать ошибок отслеживания или Access to disposed context
                using (var context = _dbContextFactory.CreateDbContext())
                {
                    userFromDb = await context.Users
                        .Include(u => u.Role)
                        .Include(u => u.Group)
                            .ThenInclude(g => g.StudyGroups!)
                                .ThenInclude(sg => sg.Subject)
                        .FirstOrDefaultAsync(u => u.UserID == currentUser.UserID);
                }

                Assert.NotNull(userFromDb); // Убеждаемся, что пользователь найден

                // Отправляем сообщение об аутентификации
                WeakReferenceMessenger.Default.Send(new UserAuthenticatedMessage(userFromDb));
                await Task.Delay(200); // Даем время на асинхронную обработку в ViewModel
            }
            return _viewModel;
        }

        // --- Тесты ---

        [Fact]
        public async Task Receive_WithAuthenticatedStudentUser_LoadsStudentDataAndGrades()
        {
            // Arrange
            SetupTest(); // Инициализируем свежее состояние для этого теста

            // Act & Assert (проверяем начальное состояние VM до аутентификации)
            // Эти assert'ы должны быть для ViewModel БЕЗ аутентифицированного пользователя
            // Если вы хотите проверить начальное состояние, создайте VM без аутентификации:
            // var initialVm = new GradeVm(_dbContextFactory);
            // Assert.Equal("Неизвестно", initialVm.StudentFullName);
            // Assert.Equal("Неизвестно", initialVm.StudentGroupName);
            // Assert.Equal("Загрузка...", initialVm.StudentSubjects);
            // Assert.Empty(initialVm.StudentGrades);


            // Act
            var vm = await CreateAndAuthenticateViewModel(_studentUser);

            // Assert
            Assert.Equal($"{_studentUser.LastName} {_studentUser.FirstName}", vm.StudentFullName);
            Assert.Equal(_group9B.GroupName, vm.StudentGroupName);
            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            Assert.NotEmpty(vm.StudentGrades);
            Assert.Contains(vm.StudentGrades, g => g.PerformanceID == _studentStm32Grade.PerformanceID);
            Assert.Contains(vm.StudentGrades, g => g.PerformanceID == _studentScratchGrade.PerformanceID);
            Assert.Equal(2, vm.StudentGrades.Count);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_LoadsCorrectSubjects()
        {
            // Arrange
            SetupTest();

            // Act
            var vm = await CreateAndAuthenticateViewModel(_studentUser);

            // Assert
            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            // Split(", ") может быть проблематичен, если будет только один предмет или если формат изменится.
            // Лучше проверить список, если ViewModel предоставляет его как список.
            // Если StudentSubjects - это просто строка, то эта проверка ок.
            Assert.Equal(2, vm.StudentSubjects.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoGrades()
        {
            // Arrange
            SetupTest(); // Начнем со свежей базы данных

            // Создаем уникальные данные для этого конкретного теста
            var tempStudentRole = new Role { RoleID = 11, RoleName = "Ученик" };
            var tempTeacherRole = new Role { RoleID = 12, RoleName = "Учитель" };
            var tempTeacherUser = new User { UserID = 112, Username = "tempTeacher", FirstName = "Тест", LastName = "Учитель", MiddleName = "Темп", RoleID = tempTeacherRole.RoleID, Role = tempTeacherRole };
            var tempGroup = new Group { GroupID = 211, GroupName = "ТестГруппа" };
            var tempStm32Subject = new Subject { SubjectID = 311, SubjectName = "Тест STM32" };
            var tempScratchSubject = new Subject { SubjectID = 312, SubjectName = "Тест Scratch" };
            var tempStm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 411,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup.GroupID,
                SubjectID = tempStm32Subject.SubjectID,
                Teacher = tempTeacherUser,
                Group = tempGroup,
                Subject = tempStm32Subject
            };
            var tempScratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 412,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup.GroupID,
                SubjectID = tempScratchSubject.SubjectID,
                Teacher = tempTeacherUser,
                Group = tempGroup,
                Subject = tempScratchSubject
            };

            var studentWithoutGrades_Test = new User { UserID = 113, Username = "nogrades", FirstName = "Тест", LastName = "БезОценок", RoleID = tempStudentRole.RoleID, GroupID = tempGroup.GroupID, Role = tempStudentRole, Group = tempGroup };

            // Сеем только необходимые данные для этого теста
            SeedDatabase(
                tempStudentRole,
                tempTeacherRole,
                tempTeacherUser,
                tempGroup,
                tempStm32Subject,
                tempScratchSubject,
                tempStm32StudyGroup,
                tempScratchStudyGroup,
                studentWithoutGrades_Test
            );
            // В этом тесте мы не сеем AcademicPerformance, чтобы проверить сценарий без оценок.

            // Act
            var vm = await CreateAndAuthenticateViewModel(studentWithoutGrades_Test);

            // Assert
            Assert.Equal($"{studentWithoutGrades_Test.LastName} {studentWithoutGrades_Test.FirstName}", vm.StudentFullName);
            Assert.Equal(tempGroup.GroupName, vm.StudentGroupName);
            Assert.Contains("Тест STM32", vm.StudentSubjects);
            Assert.Contains("Тест Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Length);
            Assert.Empty(vm.StudentGrades); // Ожидаем, что список оценок пуст
        }

        [Fact]
        public async Task GradeDisplayModel_CorrectlyMapsData()
        {
            // Arrange
            SetupTest();

            // Act
            var vm = await CreateAndAuthenticateViewModel(_studentUser);

            // Assert
            var stm32DisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentStm32Grade.PerformanceID);
            Assert.NotNull(stm32DisplayGrade);
            Assert.Equal(_stm32Subject.SubjectName, stm32DisplayGrade.SubjectName);
            // Убедитесь, что логика формирования TeacherFullName в GradeDisplayModel соответствует ожидаемой
            Assert.Equal($"{_teacherUser.LastName} {_teacherUser.FirstName[0]}.{_teacherUser.MiddleName[0]}.", stm32DisplayGrade.TeacherFullName);
            Assert.Equal(DateOnly.FromDateTime(_stm32Lesson1.LessonDate), stm32DisplayGrade.LessonDate);
            Assert.Equal(_stm32Lesson1.LessonTime, stm32DisplayGrade.LessonTime);
            Assert.Equal(_studentStm32Grade.Grade, stm32DisplayGrade.GradeValue);
            Assert.Equal(_studentStm32Grade.Attendance, stm32DisplayGrade.AttendanceMark);
            Assert.Equal(_studentStm32Grade.Comment, stm32DisplayGrade.Comment);

            var scratchDisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentScratchGrade.PerformanceID);
            Assert.NotNull(scratchDisplayGrade);
            Assert.Equal(_scratchSubject.SubjectName, scratchDisplayGrade.SubjectName);
            Assert.Equal($"{_teacherUser.LastName} {_teacherUser.FirstName[0]}.{_teacherUser.MiddleName[0]}.", scratchDisplayGrade.TeacherFullName);
            Assert.Equal(DateOnly.FromDateTime(_scratchLesson1.LessonDate), scratchDisplayGrade.LessonDate);
            Assert.Equal(_scratchLesson1.LessonTime, scratchDisplayGrade.LessonTime);
            Assert.Equal(_studentScratchGrade.Grade, scratchDisplayGrade.GradeValue);
            Assert.Equal(_studentScratchGrade.Attendance, scratchDisplayGrade.AttendanceMark);
            Assert.Equal(_studentScratchGrade.Comment, scratchDisplayGrade.Comment);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesMissingGroupOrSubjects()
        {
            // Arrange
            SetupTest(); // Начнем со свежей базы данных

            // Создаем уникального пользователя без группы и без связанных StudyGroups/Subjects
            var studentNoGroup = new User { UserID = 104, Username = "nogroup", FirstName = "Тест", LastName = "БезГруппы", RoleID = _studentRole.RoleID, Role = _studentRole };

            SeedDatabase(_studentRole, studentNoGroup); // Сеем только роль и пользователя без группы

            // Act
            var vm = await CreateAndAuthenticateViewModel(studentNoGroup);

            // Assert
            Assert.Equal($"{studentNoGroup.LastName} {studentNoGroup.FirstName}", vm.StudentFullName);
            Assert.Equal("Группа не определена", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }
    }
}