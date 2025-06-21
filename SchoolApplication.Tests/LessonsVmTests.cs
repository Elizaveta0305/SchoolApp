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
using System.Globalization;

namespace SchoolApplication.Tests
{
    public class LessonsVmTests
    {
        private readonly Role _teacherRole;
        private readonly Role _studentRole;
        private readonly User _teacherUser;
        private readonly User _studentUser9A;
        private readonly User _studentUser9B;
        private readonly User _studentUserNoGroup;
        private readonly Group _group9A;
        private readonly Group _group9B;
        private readonly Subject _mathSubject;
        private readonly Subject _physicsSubject;
        private readonly StudyGroup _math9AStudyGroup;
        private readonly StudyGroup _physics9AStudyGroup;
        private readonly StudyGroup _math9BStudyGroup;
        private readonly Classroom _classroom101;
        private readonly Classroom _classroom102;
        private readonly Lesson _lessonMath9APast;
        private readonly Lesson _lessonMath9AFuture1;
        private readonly Lesson _lessonMath9AFuture2;
        private readonly Lesson _lessonPhysics9AFuture;
        private readonly Lesson _lessonMath9BFuture;

        public LessonsVmTests()
        {
            _teacherRole = new Role { RoleID = 1, RoleName = "Преподаватель" };
            _studentRole = new Role { RoleID = 2, RoleName = "Студент" };

            _group9A = new Group { GroupID = 1, GroupName = "9A" };
            _group9B = new Group { GroupID = 2, GroupName = "9B" };

            _teacherUser = new User
            {
                UserID = 1,
                Username = "teacher",
                PasswordHash = "hash",
                FirstName = "Иван",
                LastName = "Петров",
                MiddleName = "Сергеевич",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };
            _studentUser9A = new User
            {
                UserID = 2,
                Username = "student9a",
                PasswordHash = "hash",
                FirstName = "Анна",
                LastName = "Иванова",
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                GroupID = _group9A.GroupID,
                Group = _group9A
            };
            _studentUser9B = new User
            {
                UserID = 3,
                Username = "student9b",
                PasswordHash = "hash",
                FirstName = "Мария",
                LastName = "Сидорова",
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                GroupID = _group9B.GroupID,
                Group = _group9B
            };
            _studentUserNoGroup = new User
            {
                UserID = 4,
                Username = "nogroup",
                PasswordHash = "hash",
                FirstName = "Николай",
                LastName = "Безгруппный",
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                GroupID = null
            };

            _mathSubject = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _physicsSubject = new Subject { SubjectID = 2, SubjectName = "Физика" };

            _classroom101 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            _classroom102 = new Classroom { ClassroomID = 2, RoomNumber = "102" };

            _math9AStudyGroup = new StudyGroup
            {
                StudyGroupID = 1,
                TeacherID = _teacherUser.UserID,
                Teacher = _teacherUser,
                SubjectID = _mathSubject.SubjectID,
                Subject = _mathSubject,
                GroupID = _group9A.GroupID,
                Group = _group9A
            };
            _physics9AStudyGroup = new StudyGroup
            {
                StudyGroupID = 2,
                TeacherID = _teacherUser.UserID,
                Teacher = _teacherUser,
                SubjectID = _physicsSubject.SubjectID,
                Subject = _physicsSubject,
                GroupID = _group9A.GroupID,
                Group = _group9A
            };
            _math9BStudyGroup = new StudyGroup
            {
                StudyGroupID = 3,
                TeacherID = _teacherUser.UserID,
                Teacher = _teacherUser,
                SubjectID = _mathSubject.SubjectID,
                Subject = _mathSubject,
                GroupID = _group9B.GroupID,
                Group = _group9B
            };

            _lessonMath9APast = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _math9AStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2025, 6, 20),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Математика: Прошлое",
                StudyGroup = _math9AStudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };
            _lessonMath9AFuture1 = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _math9AStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2025, 6, 26),
                LessonTime = new TimeSpan(11, 0, 0),
                Topic = "Математика: Будущее 1",
                StudyGroup = _math9AStudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };
            _lessonMath9AFuture2 = new Lesson
            {
                LessonID = 3,
                StudyGroupID = _math9AStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2025, 6, 26),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Математика: Будущее 2 (раньше)",
                StudyGroup = _math9AStudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };
            _lessonPhysics9AFuture = new Lesson
            {
                LessonID = 4,
                StudyGroupID = _physics9AStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2025, 6, 27),
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Физика: Будущее",
                StudyGroup = _physics9AStudyGroup,
                Classroom = _classroom102,
                ClassroomID = _classroom102.ClassroomID
            };
            _lessonMath9BFuture = new Lesson
            {
                LessonID = 5,
                StudyGroupID = _math9BStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2025, 6, 28),
                LessonTime = new TimeSpan(13, 0, 0),
                Topic = "Математика 9Б: Будущее",
                StudyGroup = _math9BStudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };
        }

        private async Task<LessonsVm> CreateViewModel(User currentUser, params object[] entitiesToSeed)
        {
            var dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());

            using (var context = dbContextFactory.CreateDbContext())
            {
                dbContextFactory.SeedData(context, entitiesToSeed);
                await context.SaveChangesAsync();
            }

            var messenger = new StrongReferenceMessenger();

            var vm = new LessonsVm(dbContextFactory, messenger);

            messenger.Send(new UserAuthenticatedMessage(currentUser));

            await vm._initialLoadCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));

            return vm;
        }

        [Fact]
        public async Task LoadAllStudentLessons_LoadsLessonsForAuthenticatedStudent()
        {
            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUser9A, _studentUser9B,
                _mathSubject, _physicsSubject,
                _math9AStudyGroup, _physics9AStudyGroup, _math9BStudyGroup,
                _classroom101, _classroom102,
                _lessonMath9APast, _lessonMath9AFuture1, _lessonMath9AFuture2, _lessonPhysics9AFuture, _lessonMath9BFuture
            };

            var vm = await CreateViewModel(_studentUser9A, entities.ToArray());

            Assert.NotNull(vm.AllStudentLessons);
            Assert.Equal(4, vm.AllStudentLessons.Count);

            Assert.Equal(_lessonMath9APast.LessonID, vm.AllStudentLessons[0].LessonId);
            Assert.Equal(_lessonMath9AFuture2.LessonID, vm.AllStudentLessons[1].LessonId);
            Assert.Equal(_lessonMath9AFuture1.LessonID, vm.AllStudentLessons[2].LessonId);
            Assert.Equal(_lessonPhysics9AFuture.LessonID, vm.AllStudentLessons[3].LessonId);

            var firstLesson = vm.AllStudentLessons.FirstOrDefault(l => l.LessonId == _lessonMath9APast.LessonID);
            Assert.NotNull(firstLesson);
            Assert.Equal(_mathSubject.SubjectName, firstLesson.SubjectName);
            Assert.Equal($"{_teacherUser.LastName} {_teacherUser.FirstName[0]}.{_teacherUser.MiddleName[0]}.", firstLesson.TeacherFullName); // <--- ИЗМЕНЕНО
            Assert.Equal(_classroom101.RoomNumber, firstLesson.RoomNumber);
            Assert.Equal("06/20/2025", firstLesson.FormattedLessonDate);
            Assert.Equal("10:00", firstLesson.FormattedLessonTime);

            var futureLesson = vm.AllStudentLessons.FirstOrDefault(l => l.LessonId == _lessonPhysics9AFuture.LessonID);
            Assert.NotNull(futureLesson);
            Assert.Equal(_physicsSubject.SubjectName, futureLesson.SubjectName);
            Assert.Equal($"{_teacherUser.LastName} {_teacherUser.FirstName[0]}.{_teacherUser.MiddleName[0]}.", futureLesson.TeacherFullName); // <--- ИЗМЕНЕНО
            Assert.Equal(_classroom102.RoomNumber, futureLesson.RoomNumber);
            Assert.Equal("06/27/2025", futureLesson.FormattedLessonDate);
            Assert.Equal("09:00", futureLesson.FormattedLessonTime);
        }

        [Fact]
        public async Task LoadAllStudentLessons_HandlesNoLessonsOrNoUser()
        {
            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUserNoGroup,
                _mathSubject, _physicsSubject,
                _math9AStudyGroup, _physics9AStudyGroup, _math9BStudyGroup,
                _classroom101, _classroom102,
                _lessonMath9APast, _lessonMath9AFuture1, _lessonPhysics9AFuture, _lessonMath9BFuture
            };

            var vmNoGroup = await CreateViewModel(_studentUserNoGroup, entities.ToArray());
            Assert.NotNull(vmNoGroup.AllStudentLessons);
            Assert.Empty(vmNoGroup.AllStudentLessons);

            var dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());
            var messenger = new StrongReferenceMessenger();
            var vmNullUser = new LessonsVm(dbContextFactory, messenger);

            await vmNullUser.LoadAllStudentLessonsCommand.ExecuteAsync(null);

            Assert.NotNull(vmNullUser.AllStudentLessons);
            Assert.Empty(vmNullUser.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_SortsLessonsCorrectly()
        {
            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A,
                _teacherUser, _studentUser9A,
                _mathSubject, _physicsSubject,
                _math9AStudyGroup, _physics9AStudyGroup,
                _classroom101, _classroom102,
                _lessonMath9AFuture1,
                _lessonMath9AFuture2,
                _lessonPhysics9AFuture,
                _lessonMath9APast
            };

            var vm = await CreateViewModel(_studentUser9A, entities.ToArray());

            Assert.NotNull(vm.AllStudentLessons);
            Assert.Equal(4, vm.AllStudentLessons.Count);

            Assert.Equal(_lessonMath9APast.LessonID, vm.AllStudentLessons[0].LessonId);
            Assert.Equal(_lessonMath9AFuture2.LessonID, vm.AllStudentLessons[1].LessonId);
            Assert.Equal(_lessonMath9AFuture1.LessonID, vm.AllStudentLessons[2].LessonId);
            Assert.Equal(_lessonPhysics9AFuture.LessonID, vm.AllStudentLessons[3].LessonId);
        }

        [Fact]
        public async Task LoadAllStudentLessons_FiltersByUsersGroup()
        {
            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUser9A, _studentUser9B,
                _mathSubject, _physicsSubject,
                _math9AStudyGroup, _physics9AStudyGroup, _math9BStudyGroup,
                _classroom101, _classroom102,
                _lessonMath9APast, _lessonMath9AFuture1, _lessonPhysics9AFuture,
                _lessonMath9BFuture
            };

            var vm = await CreateViewModel(_studentUser9A, entities.ToArray());

            Assert.NotNull(vm.AllStudentLessons);
            Assert.Equal(3, vm.AllStudentLessons.Count);

            Assert.DoesNotContain(vm.AllStudentLessons, l => l.LessonId == _lessonMath9BFuture.LessonID);

            Assert.True(vm.AllStudentLessons.All(l =>
                l.LessonId == _lessonMath9APast.LessonID ||
                l.LessonId == _lessonMath9AFuture1.LessonID ||
                l.LessonId == _lessonPhysics9AFuture.LessonID
            ));
        }

        [Fact]
        public async Task LoadAllStudentLessons_HandlesDatabaseExceptions()
        {
            var mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            mockDbContextFactory.Setup(f => f.CreateDbContext()).Throws(new InvalidOperationException("Test database error"));

            var messenger = new StrongReferenceMessenger();
            var vm = new LessonsVm(mockDbContextFactory.Object, messenger);
            messenger.Send(new UserAuthenticatedMessage(_studentUser9A));

            await vm.LoadAllStudentLessonsCommand.ExecuteAsync(null);

            Assert.NotNull(vm.AllStudentLessons);
            Assert.Empty(vm.AllStudentLessons);
        }
    }
}