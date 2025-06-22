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
using SchoolApplication.Models.DisplayModels;
using System;
using CommunityToolkit.Mvvm.Messaging.Messages; // Убедитесь, что эта директива using есть

namespace SchoolApplication.Tests
{
    // Используем коллекцию для мессенджера, чтобы обеспечить его сброс между тестами,
    // но при этом использовать один и тот же экземпляр для инжекции в тесты.
    [Collection("MessengerCollection")]
    public class GradeVmTests : IDisposable
    {
        private readonly TestDbContextFactory _dbContextFactory;
        private ApplicationDbContext _currentTestDbContext; // Контекст для текущего теста
        private IMessenger _messenger; // Поле для инжектированного мессенджера
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

        public GradeVmTests(MessengerFixture fixture)
        {
            _dbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger; // Инициализируем мессенджер из фикстуры
        }

        // Метод, который будет вызываться перед каждым тестом для инициализации
        private void SetupTest()
        {
            // Создаем новый, чистый DbContext для каждого теста.
            // TestDbContextFactory.CreateDbContext() должен заботиться об очистке и создании базы данных.
            _currentTestDbContext = _dbContextFactory.CreateDbContext();

            // Инициализируем тестовые данные для каждого теста
            _studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            _teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            _group9B = new Group { GroupID = 201, GroupName = "9Б" };

            _studentUser = new User
            {
                UserID = 101,
                Username = "student1",
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                RoleID = _studentRole.RoleID,
                GroupID = _group9B.GroupID,
                Role = _studentRole,
                Group = _group9B,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>() // Важно инициализировать коллекцию
            };

            _teacherUser = new User
            {
                UserID = 102,
                Username = "teacher1",
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole,
                StudyGroupsAsTeacher = new List<StudyGroup>() // Инициализируем коллекцию
            };

            _stm32Subject = new Subject { SubjectID = 301, SubjectName = "STM32 в среде STM32CubeIDE" };
            _scratchSubject = new Subject { SubjectID = 302, SubjectName = "Scratch" };

            _stm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 401,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _stm32Subject.SubjectID,
                Teacher = _teacherUser,
                Group = _group9B,
                Subject = _stm32Subject,
                Lessons = new List<Lesson>() // Инициализируем коллекцию
            };
            _scratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 402,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _scratchSubject.SubjectID,
                Teacher = _teacherUser,
                Group = _group9B,
                Subject = _scratchSubject,
                Lessons = new List<Lesson>() // Инициализируем коллекцию
            };

            // Добавляем StudyGroups к учителю (StudyGroupsAsTeacher)
            _teacherUser.StudyGroupsAsTeacher.Add(_stm32StudyGroup);
            _teacherUser.StudyGroupsAsTeacher.Add(_scratchStudyGroup);

            // Добавляем StudyGroups к группе
            _group9B.StudyGroups = new List<StudyGroup> { _stm32StudyGroup, _scratchStudyGroup };


            _stm32Lesson1 = new Lesson
            {
                LessonID = 501,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                ClassroomID = 1,
                LessonDate = new DateTime(2024, 05, 10),
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Введение в STM32CubeIDE",
                StudyGroup = _stm32StudyGroup,
                Classroom = new Classroom { ClassroomID = 1, RoomNumber = "101" }
            };
            _scratchLesson1 = new Lesson
            {
                LessonID = 502,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                ClassroomID = 2,
                LessonDate = new DateTime(2024, 05, 11),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Создание первого проекта",
                StudyGroup = _scratchStudyGroup,
                Classroom = new Classroom { ClassroomID = 2, RoomNumber = "102" }
            };

            // Добавляем уроки к StudyGroup
            _stm32StudyGroup.Lessons.Add(_stm32Lesson1);
            _scratchStudyGroup.Lessons.Add(_scratchLesson1);


            _studentStm32Grade = new AcademicPerformance
            {
                PerformanceID = 601,
                StudentID = _studentUser.UserID,
                LessonID = _stm32Lesson1.LessonID,
                Grade = "5",
                Attendance = true,
                Comment = "Отличная работа с платой",
                Student = _studentUser,
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

            // Добавляем AcademicPerformance к студенту
            _studentUser.AcademicPerformanceAsStudent.Add(_studentStm32Grade);
            _studentUser.AcademicPerformanceAsStudent.Add(_studentScratchGrade);


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
                _stm32Lesson1.Classroom, // Добавляем аудитории
                _scratchLesson1.Classroom, // Добавляем аудитории
                _studentStm32Grade,
                _studentScratchGrade
            );

            // Инициализируем ViewModel с новой фабрикой и ИНЖЕКТИРОВАННЫМ мессенджером для каждого теста
            _viewModel = new GradeVm(_dbContextFactory, _messenger);
        }

        // Метод Dispose для очистки ресурсов после каждого теста
        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
            // Мессенджер сбрасывается MessengerFixture
        }

        // Вспомогательный метод для посева данных, использующий текущий DbContext
        private void SeedDatabase(params object[] entities)
        {
            _dbContextFactory.SeedData(_currentTestDbContext, entities);
            // ChangeTracker.Clear() вызывается внутри TestDbContextFactory.SeedData
        }

        // Вспомогательный метод для создания ViewModel и отправки сообщения аутентификации
        private async Task<GradeVm> CreateAndAuthenticateViewModel(User? currentUser = null)
        {
            if (currentUser != null)
            {
                User userFromDb;
                // ИСПОЛЬЗУЕМ _currentTestDbContext для получения пользователя,
                // так как он уже содержит засеянные данные для текущего теста.
                userFromDb = await _currentTestDbContext.Users
                    .AsNoTracking() // Важно: AsNoTracking, чтобы не было конфликтов отслеживания.
                    .Include(u => u.Role)
                    .Include(u => u.Group)
                        .ThenInclude(g => g.StudyGroups!)
                            .ThenInclude(sg => sg.Subject)
                    // *** Включаем AcademicPerformanceAsStudent, как в вашей модели User ***
                    .Include(u => u.AcademicPerformanceAsStudent!)
                        .ThenInclude(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Subject) // Subject через StudyGroup урока
                    .Include(u => u.AcademicPerformanceAsStudent!)
                        .ThenInclude(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Teacher) // Teacher через StudyGroup урока
                    .FirstOrDefaultAsync(u => u.UserID == currentUser.UserID);

                // Если userFromDb все еще null, значит, данные не были корректно засеяны
                // или ID пользователя не совпадает.
                Assert.NotNull(userFromDb);

                // Отправляем сообщение об аутентификации через инжектированный мессенджер
                _messenger.Send(new UserAuthenticatedMessage(userFromDb));
                // *** УВЕЛИЧЕННАЯ ЗАДЕРЖКА ***
                await Task.Delay(1500); // Даем время на асинхронную обработку в ViewModel.
                                       // Возможно, понадобится увеличить до 1000 мс в зависимости от окружения.
            }
            return _viewModel;
        }

        // --- Тесты ---

        [Fact]
        public async Task Receive_WithAuthenticatedStudentUser_LoadsStudentDataAndGrades()
        {
            // Arrange
            SetupTest(); // Инициализируем свежее состояние для этого теста

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
            Assert.Equal(2, vm.StudentSubjects.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoGrades()
        {
            // Arrange
            _currentTestDbContext = _dbContextFactory.CreateDbContext(); // Начинаем с чистой базы данных

            var tempStudentRole = new Role { RoleID = 11, RoleName = "Ученик" };
            var tempTeacherRole = new Role { RoleID = 12, RoleName = "Учитель" };
            var tempTeacherUser = new User { UserID = 112, Username = "tempTeacher", FirstName = "Тест", LastName = "Учитель", MiddleName = "Темп", RoleID = tempTeacherRole.RoleID, Role = tempTeacherRole, StudyGroupsAsTeacher = new List<StudyGroup>() };
            var tempGroup = new Group { GroupID = 211, GroupName = "ТестГруппа", StudyGroups = new List<StudyGroup>() };
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
                Subject = tempStm32Subject,
                Lessons = new List<Lesson>()
            };
            var tempScratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 412,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup.GroupID,
                SubjectID = tempScratchSubject.SubjectID,
                Teacher = tempTeacherUser,
                Group = tempGroup,
                Subject = tempScratchSubject,
                Lessons = new List<Lesson>()
            };
            tempTeacherUser.StudyGroupsAsTeacher.Add(tempStm32StudyGroup);
            tempTeacherUser.StudyGroupsAsTeacher.Add(tempScratchStudyGroup);
            tempGroup.StudyGroups.Add(tempStm32StudyGroup);
            tempGroup.StudyGroups.Add(tempScratchStudyGroup);

            var tempClassroom1 = new Classroom { ClassroomID = 11, RoomNumber = "201" };
            var tempClassroom2 = new Classroom { ClassroomID = 12, RoomNumber = "202" };
            var tempStm32Lesson = new Lesson { LessonID = 511, StudyGroupID = tempStm32StudyGroup.StudyGroupID, ClassroomID = tempClassroom1.ClassroomID, LessonDate = new DateTime(2024, 6, 1), LessonTime = new TimeSpan(9, 0, 0), Topic = "Тест Урок 1", StudyGroup = tempStm32StudyGroup, Classroom = tempClassroom1 };
            var tempScratchLesson = new Lesson { LessonID = 512, StudyGroupID = tempScratchStudyGroup.StudyGroupID, ClassroomID = tempClassroom2.ClassroomID, LessonDate = new DateTime(2024, 6, 2), LessonTime = new TimeSpan(10, 0, 0), Topic = "Тест Урок 2", StudyGroup = tempScratchStudyGroup, Classroom = tempClassroom2 };

            tempStm32StudyGroup.Lessons.Add(tempStm32Lesson);
            tempScratchStudyGroup.Lessons.Add(tempScratchLesson);

            // Студент, у которого нет AcademicPerformance
            var studentWithoutGrades_Test = new User
            {
                UserID = 113,
                Username = "nogrades",
                FirstName = "Тест",
                LastName = "БезОценок",
                RoleID = tempStudentRole.RoleID,
                GroupID = tempGroup.GroupID,
                Role = tempStudentRole,
                Group = tempGroup,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>() // Пустая коллекция
            };

            // Сеем только необходимые данные для этого теста. Без AcademicPerformance для studentWithoutGrades_Test.
            SeedDatabase(
                tempStudentRole,
                tempTeacherRole,
                tempTeacherUser,
                tempGroup,
                tempStm32Subject,
                tempScratchSubject,
                tempStm32StudyGroup,
                tempScratchStudyGroup,
                tempClassroom1,
                tempClassroom2,
                tempStm32Lesson,
                tempScratchLesson,
                studentWithoutGrades_Test
            );

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
            SetupTest(); // Инициализируем свежее состояние для этого теста

            // Act
            var vm = await CreateAndAuthenticateViewModel(_studentUser);

            // Assert
            var stm32DisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentStm32Grade.PerformanceID);
            Assert.NotNull(stm32DisplayGrade); // Проверяем, что объект не null

            Assert.Equal(_stm32Subject.SubjectName, stm32DisplayGrade.SubjectName);
            // Убедитесь, что логика формирования TeacherFullName в GradeDisplayModel соответствует ожидаемой
            Assert.Equal($"{_teacherUser.LastName} {_teacherUser.FirstName[0]}.{_teacherUser.MiddleName[0]}.", stm32DisplayGrade.TeacherFullName);
            Assert.Equal(DateOnly.FromDateTime(_stm32Lesson1.LessonDate), stm32DisplayGrade.LessonDate);
            Assert.Equal(_stm32Lesson1.LessonTime, stm32DisplayGrade.LessonTime);
            Assert.Equal(_studentStm32Grade.Grade, stm32DisplayGrade.GradeValue);
            Assert.Equal(_studentStm32Grade.Attendance, stm32DisplayGrade.AttendanceMark);
            Assert.Equal(_studentStm32Grade.Comment, stm32DisplayGrade.Comment);

            var scratchDisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentScratchGrade.PerformanceID);
            Assert.NotNull(scratchDisplayGrade); // Проверяем, что объект не null

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
            _currentTestDbContext = _dbContextFactory.CreateDbContext(); // Начинаем с чистого контекста специально для этого теста.

            // Создаем ТОЛЬКО те сущности, которые НУЖНЫ для этого теста.
            var tempStudentRole = new Role { RoleID = 10, RoleName = "Ученик" };
            var studentNoGroup_Test = new User
            {
                UserID = 104,
                Username = "nogroup",
                FirstName = "Тест",
                LastName = "БезГруппы",
                RoleID = tempStudentRole.RoleID,
                Role = tempStudentRole,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>() // Инициализируем, даже если пусто
            };

            // Сеем только роль и пользователя без группы.
            SeedDatabase(tempStudentRole, studentNoGroup_Test);

            // Act
            var vm = await CreateAndAuthenticateViewModel(studentNoGroup_Test);

            // Assert
            Assert.Equal($"{studentNoGroup_Test.LastName} {studentNoGroup_Test.FirstName}", vm.StudentFullName);
            Assert.Equal("Группа не определена", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task Receive_WithNullUser_ResetsProperties()
        {
            // Arrange
            SetupTest(); // Инициализируем свежее состояние для этого теста

            // Аутентифицируем пользователя, чтобы ViewModel заполнилась данными
            await CreateAndAuthenticateViewModel(_studentUser);
            Assert.NotEmpty(_viewModel.StudentGrades); // Убедимся, что данные загружены

            // Act: Отправляем сообщение с null-пользователем
            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100); // Даем время на обработку. Для сброса обычно хватает меньшей задержки.

            // Assert: Проверяем, что свойства сброшены
            Assert.Equal("Неизвестно", _viewModel.StudentFullName);
            Assert.Equal("Неизвестно", _viewModel.StudentGroupName);
            Assert.Equal("Предметы не определены", _viewModel.StudentSubjects);
            Assert.Empty(_viewModel.StudentGrades);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoLessonsForStudent()
        {
            // Arrange
            _currentTestDbContext = _dbContextFactory.CreateDbContext(); // Начинаем с чистого контекста специально для этого теста.

            // Сеем ТОЛЬКО те базовые данные, которые НУЖНЫ для этого теста, БЕЗ AcademicPerformance.
            var studentNoLessons = new User
            {
                UserID = 105,
                Username = "nolessons",
                FirstName = "Тест",
                LastName = "БезУроков",
                RoleID = _studentRole.RoleID, // Используем общие поля для роли и группы
                GroupID = _group9B.GroupID,
                Role = _studentRole,
                Group = _group9B,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>() // Пустая коллекция
            };

            // Сеем все необходимые связанные сущности для studentNoLessons, кроме AcademicPerformance
            SeedDatabase(
                _studentRole,
                _teacherRole,
                _group9B,
                _teacherUser,
                _stm32Subject,
                _scratchSubject,
                _stm32StudyGroup,
                _scratchStudyGroup,
                _stm32Lesson1.Classroom,
                _stm32Lesson1,
                _scratchLesson1.Classroom,
                _scratchLesson1,
                studentNoLessons // Засеиваем только этого нового студента
            );

            // Act
            var vm = await CreateAndAuthenticateViewModel(studentNoLessons);

            // Assert
            Assert.Equal($"{studentNoLessons.LastName} {studentNoLessons.FirstName}", vm.StudentFullName);
            Assert.Equal(_group9B.GroupName, vm.StudentGroupName);
            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects); // Проверяем, что предметы группы все еще отображаются
            Assert.Empty(vm.StudentGrades); // Ожидаем, что оценок нет
        }
    }
}