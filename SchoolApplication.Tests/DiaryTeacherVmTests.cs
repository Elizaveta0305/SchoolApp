using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using SchoolApplication.Models.DisplayModels;

namespace SchoolApplication.Tests
{
    public class DiaryTeacherVmTests
    {
        private readonly IMessenger _messenger;
        private readonly TestDbContextFactory _dbContextFactory;
        private readonly Role _teacherRole;
        private readonly Role _studentRole;
        private readonly User _teacherUser;
        private readonly Group _group10A;
        private readonly Group _group11B;
        private readonly Subject _itTechSubject;
        private readonly Subject _roboticsSubject;
        private readonly StudyGroup _itTech10AStudyGroup;
        private readonly StudyGroup _robotics11BStudyGroup;
        private readonly User _studentJohn;
        private readonly User _studentJane;
        private readonly Lesson _itTechLesson1;
        private readonly Lesson _roboticsLesson1;
        private readonly AcademicPerformance _johnItTechGrade;

        public DiaryTeacherVmTests()
        {
            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();

            _dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());

            _teacherRole = new Role { RoleID = 2, RoleName = "Преподаватель" };
            _studentRole = new Role { RoleID = 3, RoleName = "Ученик" };

            _teacherUser = new User { UserID = 101, Username = "teacher1", FirstName = "Иван", LastName = "Иванов", RoleID = _teacherRole.RoleID };
            _group10A = new Group { GroupID = 1, GroupName = "10А" };
            _group11B = new Group { GroupID = 2, GroupName = "11Б" };
            _itTechSubject = new Subject { SubjectID = 1, SubjectName = "Изучение IT-технологий" };
            _roboticsSubject = new Subject { SubjectID = 2, SubjectName = "LEGO Mindstorms EV3" };

            _itTech10AStudyGroup = new StudyGroup
            {
                StudyGroupID = 1001,
                TeacherID = _teacherUser.UserID,
                GroupID = _group10A.GroupID,
                SubjectID = _itTechSubject.SubjectID,
                Teacher = _teacherUser,
                Group = _group10A,
                Subject = _itTechSubject
            };
            _robotics11BStudyGroup = new StudyGroup
            {
                StudyGroupID = 1002,
                TeacherID = _teacherUser.UserID,
                GroupID = _group11B.GroupID,
                SubjectID = _roboticsSubject.SubjectID,
                Teacher = _teacherUser,
                Group = _group11B,
                Subject = _roboticsSubject
            };

            _studentJohn = new User { UserID = 201, Username = "john", FirstName = "Иван", LastName = "Петров", RoleID = _studentRole.RoleID, GroupID = _group10A.GroupID };
            _studentJane = new User { UserID = 202, Username = "jane", FirstName = "Анна", LastName = "Сидорова", RoleID = _studentRole.RoleID, GroupID = _group10A.GroupID };

            _itTechLesson1 = new Lesson
            {
                LessonID = 301,
                StudyGroupID = _itTech10AStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2023, 10, 26),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Основы кибербезопасности",
                StudyGroup = _itTech10AStudyGroup
            };
            _roboticsLesson1 = new Lesson
            {
                LessonID = 302,
                StudyGroupID = _robotics11BStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2023, 10, 27),
                LessonTime = new TimeSpan(11, 0, 0),
                Topic = "Движение робота по линии",
                StudyGroup = _robotics11BStudyGroup
            };

            _johnItTechGrade = new AcademicPerformance
            {
                PerformanceID = 401,
                StudentID = _studentJohn.UserID,
                LessonID = _itTechLesson1.LessonID,
                Grade = "5",
                Attendance = true,
                Comment = "Отлично",
                Student = _studentJohn,
                Lesson = _itTechLesson1
            };
        }

        private async Task<DiaryTeacherVm> CreateViewModel(User? currentUser = null)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context,
                    _teacherRole, _studentRole, _teacherUser, _group10A, _group11B,
                    _itTechSubject, _roboticsSubject,
                    _itTech10AStudyGroup, _robotics11BStudyGroup,
                    _studentJohn, _studentJane,
                    _itTechLesson1, _roboticsLesson1,
                    _johnItTechGrade
                );
            }
            var vm = new DiaryTeacherVm(_dbContextFactory);
            if (currentUser != null)
            {
                vm.Receive(new UserAuthenticatedMessage(currentUser));
                await Task.Delay(50);
            }
            return vm;
        }

        [Fact]
        public async Task Receive_WithAuthenticatedTeacherUser_LoadsInitialData()
        {
            var vm = await CreateViewModel();
            Assert.Null(vm.SelectedGroup);
            Assert.Empty(vm.Groups);
            Assert.Empty(vm.Subjects);

            vm.Receive(new UserAuthenticatedMessage(_teacherUser));
            await Task.Delay(100);

            Assert.NotNull(vm.SelectedGroup);
            Assert.Equal(_group10A.GroupID, vm.SelectedGroup.GroupID);
            Assert.Contains(vm.Groups, g => g.GroupID == _group10A.GroupID);
            Assert.Contains(vm.Groups, g => g.GroupID == _group11B.GroupID);
            Assert.Contains(vm.Subjects, s => s.SubjectID == _itTechSubject.SubjectID);
            Assert.Contains(vm.Subjects, s => s.SubjectID == _roboticsSubject.SubjectID);
            Assert.NotEmpty(vm.DiaryCollection);
            Assert.NotEmpty(vm.StudentsInSelectedGroup);
            Assert.Contains(vm.StudentsInSelectedGroup, s => s.UserID == _studentJohn.UserID);
            Assert.Contains(vm.StudentsInSelectedGroup, s => s.UserID == _studentJane.UserID);
            Assert.NotEmpty(vm.LessonsForSelectedStudent);
            Assert.Contains(vm.LessonsForSelectedStudent, l => l.LessonID == _itTechLesson1.LessonID);
        }

        [Fact]
        public async Task Receive_WithNullUser_ClearsAllData()
        {
            var vm = await CreateViewModel(_teacherUser);
            Assert.NotEmpty(vm.Groups);
            Assert.NotEmpty(vm.Subjects);
            Assert.NotEmpty(vm.DiaryCollection);

            vm.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(50);

            Assert.Empty(vm.Groups);
            Assert.Empty(vm.StudentsInSelectedGroup);
            Assert.Empty(vm.LessonsForSelectedStudent);
            Assert.Empty(vm.Subjects);
            Assert.Empty(vm.DiaryCollection);
            Assert.Null(vm.SelectedGroup);
            Assert.Null(vm.SelectedStudent);
            Assert.Null(vm.SelectedLesson);
            Assert.Null(vm.SelectedSubject);
            Assert.Null(vm.SelectedGrade);
            Assert.Null(vm.CommentInput);
            Assert.Null(vm.SelectedActionType);
        }

        [Fact]
        public async Task OnSelectedGroupChanged_LoadsStudentsAndLessonsAndDiaryData()
        {
            var vm = await CreateViewModel(_teacherUser);

            vm.SelectedGroup = _group10A;
            await Task.Delay(100);

            Assert.NotEmpty(vm.StudentsInSelectedGroup);
            Assert.Contains(vm.StudentsInSelectedGroup, s => s.UserID == _studentJohn.UserID);
            Assert.Contains(vm.StudentsInSelectedGroup, s => s.UserID == _studentJane.UserID);
            Assert.NotEmpty(vm.LessonsForSelectedStudent);
            Assert.Contains(vm.LessonsForSelectedStudent, l => l.LessonID == _itTechLesson1.LessonID);
            Assert.NotEmpty(vm.DiaryCollection);
            Assert.Contains(vm.DiaryCollection, ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);
        }

        [Fact]
        public async Task PerformGradeActionAsync_AddsNewGrade_WhenActionIsAdd()
        {
            var vm = await CreateViewModel(_teacherUser);

            vm.SelectedGroup = _group10A;
            await Task.Delay(100);

            vm.SelectedStudent = vm.StudentsInSelectedGroup.FirstOrDefault(s => s.UserID == _studentJane.UserID);
            Assert.NotNull(vm.SelectedStudent);
            await Task.Delay(100);

            vm.SelectedSubject = _itTechSubject;
            await Task.Delay(100);

            vm.SelectedLesson = vm.LessonsForSelectedStudent.FirstOrDefault(l => l.LessonID == _itTechLesson1.LessonID);
            Assert.NotNull(vm.SelectedLesson);
            await Task.Delay(100);

            Assert.Empty(vm.DiaryCollection);

            vm.SelectedGrade = "4";
            vm.CommentInput = "Хорошо";
            vm.SelectedActionType = "Добавить";

            int initialCountBeforeAdd = vm.DiaryCollection.Count;

            await vm.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            Assert.Equal(initialCountBeforeAdd + 2, vm.DiaryCollection.Count);

            var newGrade = vm.DiaryCollection.FirstOrDefault(ap => ap.StudentFullName == $"{_studentJane.LastName} {_studentJane.FirstName}" && ap.LessonDescription == _itTechLesson1.Topic);
            Assert.NotNull(newGrade);
            Assert.Equal("4", newGrade.Grade);
            Assert.Equal("Хорошо", newGrade.Comment);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var addedPerformance = await context.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == _studentJane.UserID && ap.LessonID == _itTechLesson1.LessonID);
                Assert.NotNull(addedPerformance);
                Assert.Equal("4", addedPerformance.Grade);
                Assert.Equal("Хорошо", addedPerformance.Comment);
            }

            Assert.Null(vm.SelectedGrade);
            Assert.Null(vm.CommentInput);
            Assert.Null(vm.SelectedActionType);
            Assert.Null(vm.SelectedStudent);
            Assert.Null(vm.SelectedLesson);
            Assert.Null(vm.SelectedSubject);
            Assert.Null(vm.SelectedGroup);
        }

        [Fact]
        public async Task PerformGradeActionAsync_UpdatesExistingGrade_WhenActionIsUpdate()
        {
            var vm = await CreateViewModel(_teacherUser);
            Assert.NotEmpty(vm.DiaryCollection);
            var existingDisplayModel = vm.DiaryCollection.First(ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            await vm.EditGradeCommand.ExecuteAsync(existingDisplayModel);
            await Task.Delay(100);

            Assert.NotEmpty(vm.StudentsInSelectedGroup);
            Assert.Contains(vm.StudentsInSelectedGroup, s => s.UserID == _studentJohn.UserID);
            Assert.NotEmpty(vm.LessonsForSelectedStudent);
            Assert.Contains(vm.LessonsForSelectedStudent, l => l.LessonID == _itTechLesson1.LessonID);

            Assert.Equal(_studentJohn.UserID, vm.SelectedStudent?.UserID);
            Assert.Equal(_itTechLesson1.LessonID, vm.SelectedLesson?.LessonID);
            Assert.Equal(_itTechSubject.SubjectID, vm.SelectedSubject?.SubjectID);
            Assert.Equal(_group10A.GroupID, vm.SelectedGroup?.GroupID);


            vm.SelectedGrade = "3";
            vm.CommentInput = "Средне";
            vm.SelectedActionType = "Обновить";

            await vm.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            var updatedGrade = vm.DiaryCollection.FirstOrDefault(ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);
            Assert.NotNull(updatedGrade);
            Assert.Equal("3", updatedGrade.Grade);
            Assert.Equal("Средне", updatedGrade.Comment);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var performanceInDb = await context.AcademicPerformance.FindAsync(_johnItTechGrade.PerformanceID);
                Assert.NotNull(performanceInDb);
                Assert.Equal("3", performanceInDb.Grade);
                Assert.Equal("Средне", performanceInDb.Comment);
            }
        }

        [Fact]
        public async Task PerformGradeActionAsync_DeletesExistingGrade_WhenActionIsDelete()
        {
            var vm = await CreateViewModel(_teacherUser);
            Assert.NotEmpty(vm.DiaryCollection);
            var initialCount = vm.DiaryCollection.Count;
            var existingGrade = vm.DiaryCollection.First(ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            await vm.EditGradeCommand.ExecuteAsync(existingGrade);
            await Task.Delay(100);

            vm.SelectedActionType = "Удалить";

            await vm.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(100);

            Assert.Equal(initialCount - 1, vm.DiaryCollection.Count);
            Assert.DoesNotContain(vm.DiaryCollection, ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var deletedPerformance = await context.AcademicPerformance.FindAsync(_johnItTechGrade.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Null(vm.SelectedGrade);
            Assert.Null(vm.CommentInput);
            Assert.Null(vm.SelectedActionType);
            Assert.Null(vm.SelectedStudent);
            Assert.Null(vm.SelectedLesson);
            Assert.Null(vm.SelectedSubject);
            Assert.Null(vm.SelectedGroup);
        }

        [Fact]
        public async Task EditGrade_PopulatesInputFieldsWithSelectedPerformanceData()
        {
            var vm = await CreateViewModel(_teacherUser);
            Assert.NotEmpty(vm.DiaryCollection);
            var performanceToEdit = vm.DiaryCollection.First(ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            await vm.EditGradeCommand.ExecuteAsync(performanceToEdit);
            await Task.Delay(100);

            Assert.NotEmpty(vm.StudentsInSelectedGroup);
            Assert.NotEmpty(vm.LessonsForSelectedStudent);

            Assert.Equal("Обновить", vm.SelectedActionType);
            Assert.Equal(_group10A.GroupID, vm.SelectedGroup?.GroupID);
            Assert.Equal(_studentJohn.UserID, vm.SelectedStudent?.UserID);
            Assert.Equal(_itTechSubject.SubjectID, vm.SelectedSubject?.SubjectID);
            Assert.Equal(_itTechLesson1.LessonID, vm.SelectedLesson?.LessonID);
            Assert.Equal(_johnItTechGrade.Grade, vm.SelectedGrade);
            Assert.Equal(_johnItTechGrade.Comment, vm.CommentInput);
        }

        [Fact]
        public async Task EditGrade_HandlesNullPerformance()
        {
            var vm = await CreateViewModel(_teacherUser);

            await vm.EditGradeCommand.ExecuteAsync(null);
            await Task.Delay(50);

            Assert.Equal("Добавить", vm.SelectedActionType);
            Assert.Null(vm.SelectedGrade);
            Assert.Null(vm.CommentInput);
            Assert.NotNull(vm.SelectedGroup);
        }

        [Fact]
        public async Task DeleteGrade_RemovesPerformanceFromCollectionAndDb()
        {
            var vm = await CreateViewModel(_teacherUser);
            Assert.NotEmpty(vm.DiaryCollection);
            var initialCount = vm.DiaryCollection.Count;
            var performanceToDelete = vm.DiaryCollection.First(ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            await vm.DeleteGradeCommand.ExecuteAsync(performanceToDelete);
            await Task.Delay(100);

            Assert.Equal(initialCount - 1, vm.DiaryCollection.Count);
            Assert.DoesNotContain(vm.DiaryCollection, ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var deletedPerformance = await context.AcademicPerformance.FindAsync(_johnItTechGrade.PerformanceID);
                Assert.Null(deletedPerformance);
            }
            Assert.Null(vm.SelectedStudent);
            Assert.Null(vm.SelectedLesson);
            Assert.Null(vm.SelectedSubject);
            Assert.Null(vm.SelectedGrade);
            Assert.Null(vm.CommentInput);
            Assert.Null(vm.SelectedActionType);
        }

        [Fact]
        public async Task DeleteGrade_DoesNothingForNullPerformance()
        {
            var vm = await CreateViewModel(_teacherUser);
            var initialCount = vm.DiaryCollection.Count;

            await vm.DeleteGradeCommand.ExecuteAsync(null);
            await Task.Delay(10);

            Assert.Equal(initialCount, vm.DiaryCollection.Count);
        }

        [Fact]
        public async Task OnSelectedSubjectChanged_LoadsLessonsAndDiaryData()
        {
            var vm = await CreateViewModel(_teacherUser);

            vm.SelectedGroup = _group10A;
            await Task.Delay(100);

            vm.LessonsForSelectedStudent.Clear();
            vm.DiaryCollection.Clear();

            vm.SelectedSubject = _itTechSubject;
            await Task.Delay(100);

            Assert.NotEmpty(vm.LessonsForSelectedStudent);
            Assert.Contains(vm.LessonsForSelectedStudent, l => l.LessonID == _itTechLesson1.LessonID);
            Assert.NotEmpty(vm.DiaryCollection);
            Assert.Contains(vm.DiaryCollection, ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);
        }

        [Fact]
        public async Task OnSelectedLessonChanged_LoadsDiaryData()
        {
            var vm = await CreateViewModel(_teacherUser);

            vm.SelectedGroup = _group10A;
            await Task.Delay(100);

            vm.SelectedSubject = _itTechSubject;
            await Task.Delay(100);

            vm.DiaryCollection.Clear();

            vm.SelectedLesson = _itTechLesson1;
            await Task.Delay(100);

            Assert.NotEmpty(vm.DiaryCollection);
            Assert.Contains(vm.DiaryCollection, ap => ap.AcademicPerformanceId == _johnItTechGrade.PerformanceID);
        }

        [Fact]
        public async Task PerformGradeActionAsync_DoesNotAddGrade_IfRequiredFieldsAreNull()
        {
            var vm = await CreateViewModel(_teacherUser);

            var initialCount = vm.DiaryCollection.Count;

            vm.SelectedActionType = "Добавить";
            vm.SelectedStudent = null;
            vm.SelectedLesson = null;
            vm.SelectedGrade = null;
            vm.SelectedSubject = null;
            vm.SelectedGroup = null;


            await vm.PerformGradeActionCommand.ExecuteAsync(null);
            await Task.Delay(50);

            Assert.Equal(initialCount, vm.DiaryCollection.Count);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var newPerformance = await context.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == null || ap.LessonID == null || ap.Grade == null);
                Assert.Null(newPerformance);
            }
        }
    }
}