using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.Tests;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    public class DiaryTeacherVmTests : IDisposable
    {
        private readonly TestDbContextFactory _dbContextFactory;
        private readonly Mock<IMessenger> _mockMessenger;
        private DiaryTeacherVm _viewModel;

        private readonly Role _teacherRole = new Role { RoleID = 1, RoleName = "Teacher" };
        private readonly Role _studentRole = new Role { RoleID = 3, RoleName = "Student" };
        private readonly Group _group1 = new Group { GroupID = 1, GroupName = "10A" };
        private readonly Group _group2 = new Group { GroupID = 2, GroupName = "11B" };
        private readonly Subject _math = new Subject { SubjectID = 1, SubjectName = "Математика" };
        private readonly Subject _physics = new Subject { SubjectID = 2, SubjectName = "Физика" };
        private readonly Classroom _classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };

        private User _teacherUser;
        private User _student1;
        private User _student2;
        private StudyGroup _studyGroup1;
        private StudyGroup _studyGroup2;
        private Lesson _lesson1;
        private Lesson _lesson2;
        private AcademicPerformance _performance1;

        public DiaryTeacherVmTests()
        {
            _dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());
            _mockMessenger = new Mock<IMessenger>();

            // Явно указываем строковый токен для TToken
            _mockMessenger.Setup(m => m.Register<IRecipient<UserAuthenticatedMessage>, UserAuthenticatedMessage, string>( // Изменено с object на string
                                    It.IsAny<IRecipient<UserAuthenticatedMessage>>(),
                                    It.IsAny<string>(), // Изменено с object на string
                                    It.IsAny<MessageHandler<IRecipient<UserAuthenticatedMessage>, UserAuthenticatedMessage>>()))
                          .Callback<IRecipient<UserAuthenticatedMessage>, string, MessageHandler<IRecipient<UserAuthenticatedMessage>, UserAuthenticatedMessage>>((recipient, token, handler) => // Изменено с object на string
                          {
                              // Здесь ничего не делаем, так как это мок
                          });
            _mockMessenger.Setup(m => m.UnregisterAll(It.IsAny<object>()));

            _viewModel = new DiaryTeacherVm(_dbContextFactory);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _teacherUser = new User { UserID = 1, FirstName = "Иван", LastName = "Петров", Email = "teacher@school.com", RoleID = _teacherRole.RoleID, Role = _teacherRole };
            _student1 = new User { UserID = 2, FirstName = "Анна", LastName = "Иванова", Email = "anna@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID, Role = _studentRole, Group = _group1 };
            _student2 = new User { UserID = 3, FirstName = "Петр", LastName = "Сидоров", Email = "petr@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID, Role = _studentRole, Group = _group1 };

            _studyGroup1 = new StudyGroup { StudyGroupID = 1, GroupID = _group1.GroupID, SubjectID = _math.SubjectID, TeacherID = _teacherUser.UserID, Group = _group1, Subject = _math, Teacher = _teacherUser };
            _studyGroup2 = new StudyGroup { StudyGroupID = 2, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID, Group = _group2, Subject = _physics, Teacher = _teacherUser };

            _lesson1 = new Lesson { LessonID = 1, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 10), LessonTime = new TimeSpan(9, 0, 0), Topic = "Алгебра", StudyGroup = _studyGroup1, ClassroomID = _classroom1.ClassroomID };
            _lesson2 = new Lesson { LessonID = 2, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 12), LessonTime = new TimeSpan(10, 0, 0), Topic = "Геометрия", StudyGroup = _studyGroup1, ClassroomID = _classroom1.ClassroomID };

            _performance1 = new AcademicPerformance { PerformanceID = 1, StudentID = _student1.UserID, LessonID = _lesson1.LessonID, Grade = "5", Attendance = true, Comment = "Хорошо", Student = _student1, Lesson = _lesson1 };
        }

        public void Dispose()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        private void SeedDatabase(params object[] entities)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context, entities);
            }
        }

        private void SetAuthenticatedUser(User user)
        {
            _viewModel.Receive(new UserAuthenticatedMessage(user));
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsDataAndSetsTeacher()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);

            SetAuthenticatedUser(_teacherUser);
            await Task.Delay(100);

            Assert.NotNull(GetPrivateFieldValue<User>(_viewModel, "_currentTeacherUser"));
            Assert.True(_viewModel.DiaryCollection.Any());
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_NullUserClearsData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await Task.Delay(100);

            _viewModel.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

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
            SeedDatabase(_teacherRole, _studentRole, _group1, _group2, _math, _physics, _classroom1, _teacherUser, _student1, _student2, _studyGroup1, _studyGroup2, _lesson1, _lesson2, _performance1);
            SetAuthenticatedUser(_teacherUser);

            await _viewModel.LoadDiaryDataAsync();

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedGroup()
        {
            var student3 = new User { UserID = 4, FirstName = "Максим", LastName = "Козлов", Email = "max@school.com", RoleID = _studentRole.RoleID, GroupID = _group2.GroupID, Role = _studentRole, Group = _group2 };
            var studyGroup3 = new StudyGroup { StudyGroupID = 3, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID, Group = _group2, Subject = _physics, Teacher = _teacherUser };
            var lesson3 = new Lesson { LessonID = 3, StudyGroupID = studyGroup3.StudyGroupID, LessonDate = new DateTime(2025, 2, 1), LessonTime = new TimeSpan(11, 0, 0), Topic = "Оптика", StudyGroup = studyGroup3, ClassroomID = _classroom1.ClassroomID };
            var performance3 = new AcademicPerformance { PerformanceID = 2, StudentID = student3.UserID, LessonID = lesson3.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо", Student = student3, Lesson = lesson3 };

            SeedDatabase(_teacherRole, _studentRole, _group1, _group2, _math, _physics, _classroom1, _teacherUser, _student1, _student2, student3, _studyGroup1, _studyGroup2, studyGroup3, _lesson1, _lesson2, lesson3, _performance1, performance3);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;

            await _viewModel.LoadDiaryDataAsync();

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance3.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedStudent()
        {
            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student2.UserID, LessonID = _lesson1.LessonID, Grade = "3", Attendance = true, Comment = "Требует внимания", Student = _student2, Lesson = _lesson1 };

            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _student2, _studyGroup1, _lesson1, _lesson2, _performance1, performance2);
            SetAuthenticatedUser(_teacherUser);
            await Task.Delay(500);
            _viewModel.SelectedGroup = _group1;
            await Task.Delay(200);

            _viewModel.SelectedStudent = _student1;

            await Task.Delay(500);
                                   

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedSubject()
        {
            var lessonForPhysics = new Lesson { LessonID = 3, StudyGroupID = _studyGroup2.StudyGroupID, LessonDate = new DateTime(2025, 3, 1), LessonTime = new TimeSpan(14, 0, 0), Topic = "Механика", StudyGroup = _studyGroup2, ClassroomID = _classroom1.ClassroomID };
            var performanceForPhysics = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = lessonForPhysics.LessonID, Grade = "4", Attendance = true, Comment = "Активно работает", Student = _student1, Lesson = lessonForPhysics };

            SeedDatabase(_teacherRole, _studentRole, _group1, _group2, _math, _physics, _classroom1, _teacherUser, _student1, _studyGroup1, _studyGroup2, _lesson1, _lesson2, lessonForPhysics, _performance1, performanceForPhysics);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedSubject = _physics;

            await _viewModel.LoadDiaryDataAsync();

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performanceForPhysics.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedLesson()
        {
            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = _lesson2.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо", Student = _student1, Lesson = _lesson2 };

            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _lesson2, _performance1, performance2);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedLesson = _lesson1;

            await _viewModel.LoadDiaryDataAsync();

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }


        [Fact]
        public async Task OnSelectedGroupChanged_LoadsStudentsAndLessonsAndDiaryData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await _viewModel.LoadDiaryDataAsync();

            _viewModel.SelectedGroup = _group1;

            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.StudentsInSelectedGroup);
            Assert.Contains(_viewModel.StudentsInSelectedGroup, s => s.UserID == _student1.UserID);
            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.LessonID == _lesson1.LessonID);
            Assert.NotEmpty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task OnSelectedStudentChanged_LoadsDiaryData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await _viewModel.LoadDiaryDataAsync();

            _viewModel.SelectedStudent = _student1;

            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.StudentFullName == _student1.FullName);
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_LoadsLessonsForGroupAndSubjectAndDiaryData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _physics, _classroom1, _teacherUser, _student1, _studyGroup1, _studyGroup2, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            _viewModel.SelectedGroup = _group1;
            await _viewModel.LoadDiaryDataAsync();

            _viewModel.SelectedSubject = _math;

            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.StudyGroupID == _studyGroup1.StudyGroupID);
            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.SubjectName == _math.SubjectName);
        }

        [Fact]
        public async Task OnSelectedLessonChanged_LoadsDiaryData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await _viewModel.LoadDiaryDataAsync();

            _viewModel.SelectedLesson = _lesson1;

            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
        }

        [Fact]
        public async Task PerformGradeActionAsync_AddGrade_ExistingPerformance_DoesNothing()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Добавить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "2";

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.Equal("5", performance.Grade);
            }
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdateGrade_NonExistingPerformance_DoesNothing()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Обновить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "4";

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance);
            }
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeleteGrade_NonExistingPerformance_DoesNothing()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1);
            SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedActionType = "Удалить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance);
            }
        }

        [Fact]
        public async Task EditGrade_NullPerformanceSetsActionToAdd()
        {
            SetAuthenticatedUser(_teacherUser);

            await _viewModel.EditGradeCommand.ExecuteAsync(null);

            Assert.Equal("Добавить", _viewModel.SelectedActionType);
            Assert.Null(_viewModel.SelectedGrade);
            Assert.Null(_viewModel.CommentInput);
            Assert.Equal(0, GetPrivateFieldValue<int>(_viewModel, "_editingPerformanceId"));
        }

        [Fact]
        public async Task DeleteGrade_RemovesPerformanceAndReloadsData()
        {
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await _viewModel.LoadDiaryDataAsync();

            var displayModel = _viewModel.DiaryCollection.First();

            await _viewModel.DeleteGradeCommand.ExecuteAsync(displayModel);

            using (var dbContext = _dbContextFactory.CreateDbContext())
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
            SeedDatabase(_teacherRole, _studentRole, _group1, _math, _classroom1, _teacherUser, _student1, _studyGroup1, _lesson1, _performance1);
            SetAuthenticatedUser(_teacherUser);
            await _viewModel.LoadDiaryDataAsync();
            var initialCount = _viewModel.DiaryCollection.Count;

            await _viewModel.DeleteGradeCommand.ExecuteAsync(null);

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var existingPerformance = await dbContext.AcademicPerformance.FirstOrDefaultAsync();
                Assert.NotNull(existingPerformance);
            }
            Assert.Equal(initialCount, _viewModel.DiaryCollection.Count);
        }

        private T GetPrivateFieldValue<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj)!;
        }
    }
}