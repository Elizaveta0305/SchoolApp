using CommunityToolkit.Mvvm.Messaging; // Используем IMessenger из этого пространства имен
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; // Для GetPrivateFieldValue
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    // Применяем Collection для изоляции тестов, использующих Messenger.
    // Все тесты в этой коллекции будут выполняться в рамках одного экземпляра MessengerFixture.
    [Collection("MessengerCollection")]
    public class DiaryTeacherVmTests : IDisposable
    {
        private readonly TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext; // Контекст для текущего теста
        private IMessenger _messenger; // Инжектированный мессенджер из фикстуры
        private DiaryTeacherVm _viewModel;

        // Данные для инициализации
        private Role _teacherRole;
        private Role _studentRole;
        private Group _group1;
        private Group _group2;
        private Subject _math;
        private Subject _physics;
        private Classroom _classroom1;

        private User _teacherUser;
        private User _student1;
        private User _student2;
        private StudyGroup _studyGroup1;
        private StudyGroup _studyGroup2;
        private Lesson _lesson1;
        private Lesson _lesson2;
        private AcademicPerformance _performance1;

        // Конструктор теста теперь принимает MessengerFixture
        public DiaryTeacherVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }
        private void SetupTest()
        {
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();

            _teacherRole = new Role { RoleID = 1, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 3, RoleName = "Student" };
            _group1 = new Group { GroupID = 1, GroupName = "10A" };
            _group2 = new Group { GroupID = 2, GroupName = "11B" };
            _math = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _physics = new Subject { SubjectID = 2, SubjectName = "Физика" };
            _classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };

            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole, _group1, _group2, _math, _physics, _classroom1);

            _teacherUser = new User { UserID = 1, FirstName = "Иван", LastName = "Петров", Email = "teacher@school.com", RoleID = _teacherRole.RoleID, Role = _teacherRole };
            _student1 = new User { UserID = 2, FirstName = "Анна", LastName = "Иванова", Email = "anna@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID, Role = _studentRole, Group = _group1 };
            _student2 = new User { UserID = 3, FirstName = "Петр", LastName = "Сидоров", Email = "petr@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID, Role = _studentRole, Group = _group1 };

            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherUser, _student1, _student2);

            _studyGroup1 = new StudyGroup { StudyGroupID = 1, GroupID = _group1.GroupID, SubjectID = _math.SubjectID, TeacherID = _teacherUser.UserID, Group = _group1, Subject = _math, Teacher = _teacherUser };
            _studyGroup2 = new StudyGroup { StudyGroupID = 2, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID, Group = _group2, Subject = _physics, Teacher = _teacherUser };

            _testDbContextFactory.SeedData(_currentTestDbContext, _studyGroup1, _studyGroup2);

            _lesson1 = new Lesson { LessonID = 1, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 10), LessonTime = new TimeSpan(9, 0, 0), Topic = "Алгебра", StudyGroup = _studyGroup1, ClassroomID = _classroom1.ClassroomID };
            _lesson2 = new Lesson { LessonID = 2, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 12), LessonTime = new TimeSpan(10, 0, 0), Topic = "Геометрия", StudyGroup = _studyGroup1, ClassroomID = _classroom1.ClassroomID };

            _testDbContextFactory.SeedData(_currentTestDbContext, _lesson1, _lesson2);

            _performance1 = new AcademicPerformance { PerformanceID = 1, StudentID = _student1.UserID, LessonID = _lesson1.LessonID, Grade = "5", Attendance = true, Comment = "Хорошо", Student = _student1, Lesson = _lesson1 };

            _testDbContextFactory.SeedData(_currentTestDbContext, _performance1);

            _viewModel = new DiaryTeacherVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
            // Сброс мессенджера не нужен здесь, так как он управляется MessengerFixture
            // и для каждого запуска коллекции тестов предоставляется новый экземпляр,
            // или его состояние сбрасывается в MessengerFixture.Dispose().
        }

        // Вспомогательный метод для заполнения базы данных.
        private void SeedDatabase(params object[] entities)
        {
            _testDbContextFactory.SeedData(_currentTestDbContext, entities);
            _currentTestDbContext.ChangeTracker.Clear(); // Очищаем отслеживаемые сущности, чтобы избежать проблем.
        }

        // Метод для имитации аутентификации пользователя
        private async Task SetAuthenticatedUser(User user)
        {
            // Получаем пользователя с необходимыми включенными навигационными свойствами
            using (var context = _testDbContextFactory.CreateDbContext())
            {
                var userFromDb = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.Group)
                        .ThenInclude(g => g.StudyGroups!)
                            .ThenInclude(sg => sg.Subject)
                    .FirstOrDefaultAsync(u => u.UserID == user.UserID);

                // Отправляем сообщение через инжектированный мессенджер
                _messenger.Send(new UserAuthenticatedMessage(userFromDb));
                await Task.Delay(200); // Даем время на асинхронную обработку в ViewModel
            }
        }

        // Вспомогательный метод для получения значений приватных полей
        private T GetPrivateFieldValue<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj)!;
        }

        // --- Тесты ---

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsDataAndSetsTeacher()
        {
            // Arrange
            SetupTest(); // Вызываем SetupTest для инициализации свежей базы данных и ViewModel

            // Act
            await SetAuthenticatedUser(_teacherUser); // Используем await, так как SetAuthenticatedUser теперь асинхронный

            // Assert
            Assert.NotNull(GetPrivateFieldValue<User>(_viewModel, "_currentTeacherUser"));
            Assert.True(_viewModel.DiaryCollection.Any());
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_NullUserClearsData()
        {
            // Arrange
            SetupTest();

            await SetAuthenticatedUser(_teacherUser);
            Assert.NotEmpty(_viewModel.DiaryCollection); // Убедимся, что данные загружены

            // Act
            _messenger.Send(new UserAuthenticatedMessage(null)); // Отправляем сообщение с null пользователем
            await Task.Delay(100);

            // Assert
            Assert.Null(GetPrivateFieldValue<User>(_viewModel, "_currentTeacherUser"));
            Assert.Empty(_viewModel.DiaryCollection);
            Assert.Empty(_viewModel.Groups);
            Assert.Empty(_viewModel.StudentsInSelectedGroup);
            Assert.Empty(_viewModel.LessonsForSelectedStudent);
            Assert.Empty(_viewModel.Subjects);
            Assert.Null(_viewModel.SelectedGroup);
            Assert.Null(_viewModel.SelectedStudent);
            Assert.Null(_viewModel.SelectedLesson);
            Assert.Null(_viewModel.SelectedSubject);
            Assert.Null(_viewModel.SelectedGrade);
            Assert.Null(_viewModel.CommentInput);
            Assert.Null(_viewModel.SelectedActionType);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_LoadsCorrectDataForTeacher()
        {
            // Arrange
            SetupTest();

            await SetAuthenticatedUser(_teacherUser); // Авторизуем пользователя, чтобы ViewModel мог загрузить данные

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedGroup()
        {
            // Arrange
            SetupTest();

            // Добавляем дополнительные данные для этого теста
            var student3 = new User { UserID = 4, FirstName = "Максим", LastName = "Козлов", Email = "max@school.com", RoleID = _studentRole.RoleID, GroupID = _group2.GroupID, Role = _studentRole, Group = _group2 };
            var studyGroup3 = new StudyGroup { StudyGroupID = 3, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID, Group = _group2, Subject = _physics, Teacher = _teacherUser };
            var lesson3 = new Lesson { LessonID = 3, StudyGroupID = studyGroup3.StudyGroupID, LessonDate = new DateTime(2025, 2, 1), LessonTime = new TimeSpan(11, 0, 0), Topic = "Оптика", StudyGroup = studyGroup3, ClassroomID = _classroom1.ClassroomID };
            var performance3 = new AcademicPerformance { PerformanceID = 2, StudentID = student3.UserID, LessonID = lesson3.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо", Student = student3, Lesson = lesson3 };

            SeedDatabase(student3, studyGroup3, lesson3, performance3);

            await SetAuthenticatedUser(_teacherUser);

            // Act - Установка SelectedGroup должна вызвать LoadDiaryDataAsync внутри ViewModel
            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100); // Ждем завершения асинхронной операции

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance3.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedStudent()
        {
            // Arrange
            SetupTest();

            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student2.UserID, LessonID = _lesson1.LessonID, Grade = "3", Attendance = true, Comment = "Требует внимания", Student = _student2, Lesson = _lesson1 };
            SeedDatabase(performance2);

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            // Act
            _viewModel.SelectedStudent = _student1;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedSubject()
        {
            // Arrange
            SetupTest();

            var lessonForPhysics = new Lesson { LessonID = 3, StudyGroupID = _studyGroup2.StudyGroupID, LessonDate = new DateTime(2025, 3, 1), LessonTime = new TimeSpan(14, 0, 0), Topic = "Механика", StudyGroup = _studyGroup2, ClassroomID = _classroom1.ClassroomID };
            var performanceForPhysics = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = lessonForPhysics.LessonID, Grade = "4", Attendance = true, Comment = "Активно работает", Student = _student1, Lesson = lessonForPhysics };

            SeedDatabase(lessonForPhysics, performanceForPhysics);

            await SetAuthenticatedUser(_teacherUser);

            // Act
            _viewModel.SelectedSubject = _physics;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performanceForPhysics.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedLesson()
        {
            // Arrange
            SetupTest();

            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = _lesson2.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо", Student = _student1, Lesson = _lesson2 };
            SeedDatabase(performance2);

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            // Act
            _viewModel.SelectedLesson = _lesson1;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }


        [Fact]
        public async Task OnSelectedGroupChanged_LoadsStudentsAndLessonsAndDiaryData()
        {
            // Arrange
            SetupTest();

            await SetAuthenticatedUser(_teacherUser);

            // Act
            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.StudentsInSelectedGroup);
            Assert.Contains(_viewModel.StudentsInSelectedGroup, s => s.UserID == _student1.UserID);
            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.LessonID == _lesson1.LessonID);
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
        }

        [Fact]
        public async Task OnSelectedStudentChanged_LoadsDiaryData()
        {
            // Arrange
            SetupTest();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            // Act
            _viewModel.SelectedStudent = _student1;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.StudentFullName == _student1.FullName);
            Assert.Equal(1, _viewModel.DiaryCollection.Count(ap => ap.StudentFullName == _student1.FullName));
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_LoadsLessonsForGroupAndSubjectAndDiaryData()
        {
            // Arrange
            SetupTest();

            var lessonPhysicsForGroup1 = new Lesson { LessonID = 10, StudyGroupID = _studyGroup2.StudyGroupID, LessonDate = new DateTime(2025, 4, 1), LessonTime = new TimeSpan(13, 0, 0), Topic = "Физика для 10А", ClassroomID = _classroom1.ClassroomID, StudyGroup = _studyGroup2, Classroom = _classroom1 };
            SeedDatabase(lessonPhysicsForGroup1);

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            // Act
            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.StudyGroupID == _studyGroup1.StudyGroupID);
            Assert.DoesNotContain(_viewModel.LessonsForSelectedStudent, l => l.StudyGroupID == _studyGroup2.StudyGroupID);
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.SubjectName == _math.SubjectName);
        }

        [Fact]
        public async Task OnSelectedLessonChanged_LoadsDiaryData()
        {
            // Arrange
            SetupTest();

            var performance2ForLesson2 = new AcademicPerformance { PerformanceID = 20, StudentID = _student1.UserID, LessonID = _lesson2.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо", Student = _student1, Lesson = _lesson2 };
            SeedDatabase(performance2ForLesson2);

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            // Act
            _viewModel.SelectedLesson = _lesson1;
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson2.Topic);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task PerformGradeActionAsync_AddGrade_ExistingPerformance_DoesNothing()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Добавить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "2";

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.NotNull(performance);
                Assert.Equal("5", performance.Grade);
            }
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID && ap.Grade == "5");
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task PerformGradeActionAsync_AddGrade_NewPerformance_AddsGrade()
        {
            // Arrange
            SetupTest();
            // В этом тесте мы не засеиваем _performance1, чтобы симулировать создание новой записи
            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1);
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Добавить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "4";
            _viewModel.CommentInput = "Новая оценка";

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var newPerformance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.NotNull(newPerformance);
                Assert.Equal("4", newPerformance.Grade);
                Assert.Equal("Новая оценка", newPerformance.Comment);
                Assert.True(newPerformance.Attendance);
            }
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.StudentFullName == _student1.FullName && ap.LessonId == _lesson1.LessonID && ap.Grade == "4");
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdateGrade_NonExistingPerformance_DoesNothing()
        {
            // Arrange
            SetupTest();
            // Убеждаемся, что _performance1 не засеян для этого теста
            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1);
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Обновить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "4";

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance); // Должен остаться null
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdateGrade_ExistingPerformance_UpdatesGrade()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            await _viewModel.LoadDiaryDataAsync();
            var displayModelToEdit = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            await _viewModel.EditGradeCommand.ExecuteAsync(displayModelToEdit);

            _viewModel.SelectedActionType = "Обновить";
            _viewModel.SelectedGrade = "4";
            _viewModel.CommentInput = "Обновленный комментарий";

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var updatedPerformance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.NotNull(updatedPerformance);
                Assert.Equal("4", updatedPerformance.Grade);
                Assert.Equal("Обновленный комментарий", updatedPerformance.Comment);
            }
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID && ap.Grade == "4" && ap.Comment == "Обновленный комментарий");
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeleteGrade_NonExistingPerformance_DoesNothing()
        {
            // Arrange
            SetupTest();
            // Убеждаемся, что _performance1 не засеян для этого теста
            _testDbContextFactory.SeedData(_currentTestDbContext, _teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1);
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Удалить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance); // Должен остаться null
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeleteGrade_ExistingPerformance_DeletesGrade()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Удалить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;

            // Act
            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var deletedPerformance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task EditGrade_NullPerformanceSetsActionToAdd()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            // Act
            await _viewModel.EditGradeCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Добавить", _viewModel.SelectedActionType);
            Assert.Null(_viewModel.SelectedGrade);
            Assert.Null(_viewModel.CommentInput);
            Assert.Equal(0, GetPrivateFieldValue<int>(_viewModel, "_editingPerformanceId"));
        }

        [Fact]
        public async Task EditGrade_ExistingPerformanceSetsActionToUpdateAndPopulatesFields()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            var displayModel = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            // Act
            await _viewModel.EditGradeCommand.ExecuteAsync(displayModel);
            await Task.Delay(100);

            // Assert
            Assert.Equal("Обновить", _viewModel.SelectedActionType);
            Assert.Equal(_performance1.Grade, _viewModel.SelectedGrade);
            Assert.Equal(_performance1.Comment, _viewModel.CommentInput);
            Assert.Equal(_performance1.PerformanceID, GetPrivateFieldValue<int>(_viewModel, "_editingPerformanceId"));
        }

        [Fact]
        public async Task DeleteGrade_RemovesPerformanceAndReloadsData()
        {
            // Arrange
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            var displayModel = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            // Act
            await _viewModel.DeleteGradeCommand.ExecuteAsync(displayModel);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var deletedPerformance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task DeleteGrade_NullPerformance_DoesNothing()
        {
            // Arrange
            SetupTest();
            SeedDatabase(_performance1);
            await SetAuthenticatedUser(_teacherUser);
            var initialCount = _viewModel.DiaryCollection.Count;

            // Act
            await _viewModel.DeleteGradeCommand.ExecuteAsync(null);
            await Task.Delay(100);

            // Assert
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var existingPerformance = await dbContext.AcademicPerformance.FirstOrDefaultAsync();
                Assert.NotNull(existingPerformance);
            }
            Assert.Equal(initialCount, _viewModel.DiaryCollection.Count);
        }
    }
}