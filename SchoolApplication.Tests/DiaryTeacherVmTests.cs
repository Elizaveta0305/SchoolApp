using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.EntityFrameworkCore;
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
    public class DiaryTeacherVmTests : IDisposable
    {
        private TestDbContextFactory _testDbContextFactory;
        private ApplicationDbContext _currentTestDbContext;
        private IMessenger _messenger;
        private DiaryTeacherVm _viewModel;

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

        public DiaryTeacherVmTests(MessengerFixture fixture)
        {
            _testDbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }
        private void SetupTest()
        {
            _currentTestDbContext = _testDbContextFactory.CreateDbContext();
 
            _currentTestDbContext.Database.EnsureDeleted();
            _currentTestDbContext.Database.EnsureCreated();

            _teacherRole = new Role { RoleID = 1, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 3, RoleName = "Student" };
            _group1 = new Group { GroupID = 1, GroupName = "10A" };
            _group2 = new Group { GroupID = 2, GroupName = "11B" };
            _math = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _physics = new Subject { SubjectID = 2, SubjectName = "Физика" };
            _classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };

            _currentTestDbContext.Roles.AddRange(_teacherRole, _studentRole);
            _currentTestDbContext.Groups.AddRange(_group1, _group2);
            _currentTestDbContext.Subjects.AddRange(_math, _physics);
            _currentTestDbContext.Classrooms.Add(_classroom1);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _teacherUser = new User { UserID = 1, FirstName = "Иван", LastName = "Петров", Email = "teacher@school.com", RoleID = _teacherRole.RoleID };
            _student1 = new User { UserID = 2, FirstName = "Анна", LastName = "Иванова", Email = "anna@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID };
            _student2 = new User { UserID = 3, FirstName = "Петр", LastName = "Сидоров", Email = "petr@school.com", RoleID = _studentRole.RoleID, GroupID = _group1.GroupID };

            _currentTestDbContext.Users.AddRange(_teacherUser, _student1, _student2);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _studyGroup1 = new StudyGroup { StudyGroupID = 1, GroupID = _group1.GroupID, SubjectID = _math.SubjectID, TeacherID = _teacherUser.UserID };
            _studyGroup2 = new StudyGroup { StudyGroupID = 2, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID };

            _currentTestDbContext.StudyGroups.AddRange(_studyGroup1, _studyGroup2);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _lesson1 = new Lesson { LessonID = 1, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 10), LessonTime = new TimeSpan(9, 0, 0), Topic = "Алгебра", ClassroomID = _classroom1.ClassroomID };
            _lesson2 = new Lesson { LessonID = 2, StudyGroupID = _studyGroup1.StudyGroupID, LessonDate = new DateTime(2025, 1, 12), LessonTime = new TimeSpan(10, 0, 0), Topic = "Геометрия", ClassroomID = _classroom1.ClassroomID };

            _currentTestDbContext.Lessons.AddRange(_lesson1, _lesson2);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _performance1 = new AcademicPerformance { PerformanceID = 1, StudentID = _student1.UserID, LessonID = _lesson1.LessonID, Grade = "5", Attendance = true, Comment = "Хорошо" };

            _currentTestDbContext.AcademicPerformance.Add(_performance1);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _viewModel = new DiaryTeacherVm(_testDbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
        }

        private async Task SetAuthenticatedUser(User user)
        {
            using (var context = _testDbContextFactory.CreateDbContext())
            {
                var userFromDb = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.Group!)
                        .ThenInclude(g => g.StudyGroups!)
                            .ThenInclude(sg => sg.Subject)
                    .FirstOrDefaultAsync(u => u.UserID == user.UserID);

                _messenger.Send(new UserAuthenticatedMessage(userFromDb));
                await Task.Delay(200);
            }
        }

        private T GetPrivateFieldValue<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj)!;
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_LoadsDataAndSetsTeacher()
        {
            SetupTest();

            await SetAuthenticatedUser(_teacherUser);

            Assert.NotNull(GetPrivateFieldValue<User>(_viewModel, "_currentTeacherUser"));
            Assert.True(_viewModel.DiaryCollection.Any());
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_NullUserClearsData()
        {
            SetupTest();

            await SetAuthenticatedUser(_teacherUser);
            Assert.NotEmpty(_viewModel.DiaryCollection);

            _messenger.Send(new UserAuthenticatedMessage(null));
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
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedGroup()
        {
            SetupTest();

            var student3 = new User { UserID = 4, FirstName = "Максим", LastName = "Козлов", Email = "max@school.com", RoleID = _studentRole.RoleID, GroupID = _group2.GroupID };
            var studyGroup3 = new StudyGroup { StudyGroupID = 3, GroupID = _group2.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID };
            var lesson3 = new Lesson { LessonID = 3, StudyGroupID = studyGroup3.StudyGroupID, LessonDate = new DateTime(2025, 2, 1), LessonTime = new TimeSpan(11, 0, 0), Topic = "Оптика", ClassroomID = _classroom1.ClassroomID };
            var performance3 = new AcademicPerformance { PerformanceID = 2, StudentID = student3.UserID, LessonID = lesson3.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо" };

            await _currentTestDbContext.Users.AddAsync(student3);
            await _currentTestDbContext.StudyGroups.AddAsync(studyGroup3);
            await _currentTestDbContext.Lessons.AddAsync(lesson3);
            await _currentTestDbContext.AcademicPerformance.AddAsync(performance3);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance3.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedStudent()
        {
            SetupTest();

            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student2.UserID, LessonID = _lesson1.LessonID, Grade = "3", Attendance = true, Comment = "Требует внимания" };
            await _currentTestDbContext.AcademicPerformance.AddAsync(performance2);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            _viewModel.SelectedStudent = _student1;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedSubject()
        {
            SetupTest();

            var lessonForPhysics = new Lesson { LessonID = 3, StudyGroupID = _studyGroup2.StudyGroupID, LessonDate = new DateTime(2025, 3, 1), LessonTime = new TimeSpan(14, 0, 0), Topic = "Механика", ClassroomID = _classroom1.ClassroomID };
            var performanceForPhysics = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = lessonForPhysics.LessonID, Grade = "4", Attendance = true, Comment = "Активно работает" };

            await _currentTestDbContext.Lessons.AddAsync(lessonForPhysics);
            await _currentTestDbContext.AcademicPerformance.AddAsync(performanceForPhysics);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedSubject = _physics;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performanceForPhysics.PerformanceID);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task LoadDiaryDataAsync_FiltersBySelectedLesson()
        {
            SetupTest();

            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student1.UserID, LessonID = _lesson2.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо" };
            await _currentTestDbContext.AcademicPerformance.AddAsync(performance2);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            _viewModel.SelectedLesson = _lesson1;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }


        [Fact]
        public async Task OnSelectedGroupChanged_LoadsStudentsAndLessonsAndDiaryData()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

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
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            var performance2 = new AcademicPerformance { PerformanceID = 2, StudentID = _student2.UserID, LessonID = _lesson1.LessonID, Grade = "3", Attendance = true, Comment = "Требует внимания" };
            await _currentTestDbContext.AcademicPerformance.AddAsync(performance2);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            _viewModel.SelectedStudent = _student1;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.StudentFullName == _student1.FullName);
            Assert.Equal(1, _viewModel.DiaryCollection.Count(ap => ap.StudentFullName == _student1.FullName));
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_LoadsLessonsForGroupAndSubjectAndDiaryData()
        {
            SetupTest();

            var studyGroupForGroup1Physics = new StudyGroup { StudyGroupID = 10, GroupID = _group1.GroupID, SubjectID = _physics.SubjectID, TeacherID = _teacherUser.UserID };
            var lessonPhysicsForGroup1 = new Lesson { LessonID = 10, StudyGroupID = studyGroupForGroup1Physics.StudyGroupID, LessonDate = new DateTime(2025, 4, 1), LessonTime = new TimeSpan(13, 0, 0), Topic = "Физика для 10А", ClassroomID = _classroom1.ClassroomID };
            var performancePhysicsForGroup1 = new AcademicPerformance { PerformanceID = 10, StudentID = _student1.UserID, LessonID = lessonPhysicsForGroup1.LessonID, Grade = "4", Attendance = true, Comment = "Тест по физике" };

            await _currentTestDbContext.StudyGroups.AddAsync(studyGroupForGroup1Physics);
            await _currentTestDbContext.Lessons.AddAsync(lessonPhysicsForGroup1);
            await _currentTestDbContext.AcademicPerformance.AddAsync(performancePhysicsForGroup1);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            await Task.Delay(100);

            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.StudyGroupID == _studyGroup1.StudyGroupID);
            Assert.DoesNotContain(_viewModel.LessonsForSelectedStudent, l => l.StudyGroupID == studyGroupForGroup1Physics.StudyGroupID);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.SubjectName == _math.SubjectName);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.SubjectName == _physics.SubjectName);
        }

        [Fact]
        public async Task OnSelectedLessonChanged_LoadsDiaryData()
        {
            SetupTest();

            var performance2ForLesson2 = new AcademicPerformance { PerformanceID = 20, StudentID = _student1.UserID, LessonID = _lesson2.LessonID, Grade = "4", Attendance = true, Comment = "Хорошо" };
            await _currentTestDbContext.AcademicPerformance.AddAsync(performance2ForLesson2);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            await SetAuthenticatedUser(_teacherUser);

            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedSubject = _math;
            await Task.Delay(100);

            _viewModel.SelectedLesson = _lesson1;
            await Task.Delay(100);

            Assert.NotEmpty(_viewModel.DiaryCollection);
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.LessonDescription == _lesson1.Topic);
            Assert.DoesNotContain(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == performance2ForLesson2.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }


        [Fact]
        public async Task PerformGradeActionAsync_AddGrade_ExistingPerformance_DoesNothing()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            _viewModel.SelectedActionType = "Добавить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "2";

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(200);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.NotNull(performance);
                Assert.Equal("5", performance.Grade);
            }
            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID && ap.Grade == "5");
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdateGrade_NonExistingPerformance_DoesNothing()
        {
            SetupTest();

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var existingPerformance = await dbContext.AcademicPerformance.FindAsync(_performance1.PerformanceID);
                if (existingPerformance != null)
                {
                    dbContext.AcademicPerformance.Remove(existingPerformance);
                    await dbContext.SaveChangesAsync();
                }
            }

            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);
            Assert.Empty(_viewModel.DiaryCollection);

            _viewModel.SelectedActionType = "Обновить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;
            _viewModel.SelectedGrade = "4";

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(200);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdateGrade_ExistingPerformance_UpdatesGrade()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            var displayModelToEdit = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            await _viewModel.EditGradeCommand.ExecuteAsync(displayModelToEdit);
            await Task.Delay(100);

            _viewModel.SelectedActionType = "Обновить";
            _viewModel.SelectedGrade = "4";
            _viewModel.CommentInput = "Обновленный комментарий";

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(500);

            await _viewModel.LoadDiaryDataAsync();
            await Task.Delay(100);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                dbContext.ChangeTracker.Clear();

                var updatedPerformance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.NotNull(updatedPerformance);
                Assert.Equal("4", updatedPerformance.Grade);
                Assert.Equal("Обновленный комментарий", updatedPerformance.Comment);
            }

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            Assert.Equal(1, _viewModel.DiaryCollection.Count);
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeleteGrade_NonExistingPerformance_DoesNothing()
        {
            SetupTest();
            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var existingPerformance = await dbContext.AcademicPerformance.FindAsync(_performance1.PerformanceID);
                if (existingPerformance != null)
                {
                    dbContext.AcademicPerformance.Remove(existingPerformance);
                    await dbContext.SaveChangesAsync();
                }
            }
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);
            Assert.Empty(_viewModel.DiaryCollection);


            _viewModel.SelectedActionType = "Удалить";
            _viewModel.SelectedGroup = _group1;
            _viewModel.SelectedStudent = _student1;
            _viewModel.SelectedLesson = _lesson1;
            _viewModel.SelectedSubject = _math;

            await _viewModel.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(200);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var performance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.StudentID == _student1.UserID && ap.LessonID == _lesson1.LessonID);
                Assert.Null(performance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeleteGrade_ExistingPerformance_DeletesGrade()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            var displayModel = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            await _viewModel.DeleteGradeCommand.ExecuteAsync(displayModel);
            await Task.Delay(500);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                dbContext.ChangeTracker.Clear();
                var deletedPerformance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
        }

        [Fact]
        public async Task EditGrade_NullPerformanceSetsActionToAdd()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);

            await _viewModel.EditGradeCommand.ExecuteAsync(null);

            Assert.Equal("Добавить", _viewModel.SelectedActionType);
            Assert.Null(_viewModel.SelectedGrade);
            Assert.Null(_viewModel.CommentInput);
            Assert.Equal(0, GetPrivateFieldValue<int>(_viewModel, "_editingPerformanceId"));

            Assert.Null(_viewModel.SelectedGroup);
            Assert.Null(_viewModel.SelectedStudent);
            Assert.Null(_viewModel.SelectedLesson);
            Assert.Null(_viewModel.SelectedSubject);
            Assert.Empty(_viewModel.StudentsInSelectedGroup);
            Assert.Empty(_viewModel.LessonsForSelectedStudent);
        }

        [Fact]
        public async Task EditGrade_ExistingPerformanceSetsActionToUpdateAndPopulatesFields()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            var displayModel = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            await _viewModel.EditGradeCommand.ExecuteAsync(displayModel);
            await Task.Delay(100);

            Assert.Equal("Обновить", _viewModel.SelectedActionType);
            Assert.Equal(_performance1.Grade, _viewModel.SelectedGrade);
            Assert.Equal(_performance1.Comment, _viewModel.CommentInput);
            Assert.Equal(_performance1.PerformanceID, GetPrivateFieldValue<int>(_viewModel, "_editingPerformanceId"));

            Assert.NotNull(_viewModel.SelectedGroup);
            Assert.Equal(_group1.GroupID, _viewModel.SelectedGroup!.GroupID);
            Assert.NotNull(_viewModel.SelectedStudent);
            Assert.Equal(_student1.UserID, _viewModel.SelectedStudent!.UserID);
            Assert.NotNull(_viewModel.SelectedLesson);
            Assert.Equal(_lesson1.LessonID, _viewModel.SelectedLesson!.LessonID);
            Assert.NotNull(_viewModel.SelectedSubject);
            Assert.Equal(_math.SubjectID, _viewModel.SelectedSubject!.SubjectID);

            Assert.NotEmpty(_viewModel.StudentsInSelectedGroup);
            Assert.Contains(_viewModel.StudentsInSelectedGroup, s => s.UserID == _student1.UserID);
            Assert.NotEmpty(_viewModel.LessonsForSelectedStudent);
            Assert.Contains(_viewModel.LessonsForSelectedStudent, l => l.LessonID == _lesson1.LessonID);
        }

        [Fact]
        public async Task DeleteGrade_RemovesPerformanceAndReloadsData()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);

            Assert.Contains(_viewModel.DiaryCollection, ap => ap.AcademicPerformanceId == _performance1.PerformanceID);
            var displayModel = _viewModel.DiaryCollection.First(ap => ap.AcademicPerformanceId == _performance1.PerformanceID);

            await _viewModel.DeleteGradeCommand.ExecuteAsync(displayModel);
            await Task.Delay(500);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                dbContext.ChangeTracker.Clear();
                var deletedPerformance = await dbContext.AcademicPerformance
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Empty(_viewModel.DiaryCollection);
            Assert.Null(_viewModel.SelectedGrade);
            Assert.Null(_viewModel.CommentInput);
            Assert.Null(_viewModel.SelectedActionType);
            Assert.Null(_viewModel.SelectedGroup);
            Assert.Null(_viewModel.SelectedStudent);
            Assert.Null(_viewModel.SelectedLesson);
            Assert.Null(_viewModel.SelectedSubject);
            Assert.Empty(_viewModel.StudentsInSelectedGroup);
            Assert.Empty(_viewModel.LessonsForSelectedStudent);
        }

        [Fact]
        public async Task DeleteGrade_NullPerformance_DoesNothing()
        {
            SetupTest();
            await SetAuthenticatedUser(_teacherUser);
            await Task.Delay(200);
            var initialCount = _viewModel.DiaryCollection.Count;
            Assert.True(initialCount > 0);

            await _viewModel.DeleteGradeCommand.ExecuteAsync(null);
            await Task.Delay(100);

            using (var dbContext = _testDbContextFactory.CreateDbContext())
            {
                var existingPerformance = await dbContext.AcademicPerformance.AsNoTracking().FirstOrDefaultAsync(ap => ap.PerformanceID == _performance1.PerformanceID);
                Assert.NotNull(existingPerformance);
            }
            Assert.Equal(initialCount, _viewModel.DiaryCollection.Count);
        }
    }
}