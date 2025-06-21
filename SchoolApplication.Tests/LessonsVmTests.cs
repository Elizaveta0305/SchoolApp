using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
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
    public class LessonsVmTests
    {
        private TestDbContextFactory CreateFactoryWithSeededData(params object[] entities)
        {
            var factory = new TestDbContextFactory();
            using var context = factory.CreateDbContext();
            factory.SeedData(context, entities);
            return factory;
        }

        [Fact]
        public async Task LoadAllStudentLessons_LoadsLessons_WhenUserHasGroupId()
        {
            var teacherUser = new User
            {
                UserID = 1,
                LastName = "Ivanov",
                FirstName = "Alexey",
                MiddleName = "V",
                RoleID = 2
            };

            var subject = new Subject
            {
                SubjectID = 1,
                SubjectName = "Physics"
            };

            var studyGroup = new StudyGroup
            {
                StudyGroupID = 1,
                GroupID = 10,
                SubjectID = subject.SubjectID,
                Subject = subject,
                TeacherID = teacherUser.UserID,
                Teacher = teacherUser
            };

            var classroom = new Classroom
            {
                ClassroomID = 1,
                RoomNumber = "101"
            };

            var lesson = new Lesson
            {
                LessonID = 1,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(9, 0, 0),
                ClassroomID = classroom.ClassroomID,
                Classroom = classroom,
                StudyGroupID = studyGroup.StudyGroupID,
                StudyGroup = studyGroup
            };

            var factory = CreateFactoryWithSeededData(teacherUser, subject, studyGroup, classroom, lesson);
            var vm = new LessonsVm(factory, WeakReferenceMessenger.Default);

            var studentUser = new User { GroupID = 10 };
            typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(vm, studentUser);

            await vm.LoadAllStudentLessons();

            Assert.Single(vm.AllStudentLessons);
            var loadedLesson = vm.AllStudentLessons.First();

            Assert.Equal(1, loadedLesson.LessonId);
            Assert.Equal("Physics", loadedLesson.SubjectName);
            Assert.Equal("Ivanov A.V.", loadedLesson.TeacherFullName);
            Assert.Equal("101", loadedLesson.RoomNumber);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserIsNull()
        {
            var factory = new TestDbContextFactory();
            var vm = new LessonsVm(factory);

            typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(vm, null);

            vm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });

            await vm.LoadAllStudentLessons();

            Assert.Empty(vm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_ClearsLessons_WhenCurrentUserGroupIdIsNull()
        {
            var factory = new TestDbContextFactory();
            var vm = new LessonsVm(factory);

            var userWithoutGroup = new User { GroupID = null };
            typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(vm, userWithoutGroup);

            vm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });

            await vm.LoadAllStudentLessons();

            Assert.Empty(vm.AllStudentLessons);
        }

        [Fact]
        public async Task LoadAllStudentLessons_HandlesExceptionAndClearsLessons()
        {
            var mockFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            mockFactory.Setup(f => f.CreateDbContext()).Throws(new Exception("DB failure"));

            var vm = new LessonsVm(mockFactory.Object);

            var user = new User { GroupID = 10 };
            typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(vm, user);

            vm.AllStudentLessons.Add(new LessonDisplayModel { LessonId = 999 });

            await vm.LoadAllStudentLessons();

            Assert.Empty(vm.AllStudentLessons);
        }

        [Fact]
        public async Task UserAuthenticatedMessage_TriggersLoadAllStudentLessons()
        {
            var factory = new TestDbContextFactory();
            var messenger = WeakReferenceMessenger.Default;
            var vm = new LessonsVm(factory, messenger);

            var tcs = vm._initialLoadCompletionSource;
            var studentUser = new User { GroupID = 10 };

            messenger.Send(new UserAuthenticatedMessage(studentUser));
            await tcs.Task;

            var currentUser = (User)typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(vm);
            Assert.Equal(studentUser.GroupID, currentUser.GroupID);
        }

        [Fact]
        public async Task TeacherFullNameFormat_WithoutMiddleName()
        {
            var teacherUser = new User
            {
                UserID = 1,
                LastName = "Petrov",
                FirstName = "Ivan",
                MiddleName = null,
                RoleID = 2
            };

            var subject = new Subject
            {
                SubjectID = 1,
                SubjectName = "Math"
            };

            var studyGroup = new StudyGroup
            {
                StudyGroupID = 1,
                GroupID = 11,
                SubjectID = subject.SubjectID,
                Subject = subject,
                TeacherID = teacherUser.UserID,
                Teacher = teacherUser
            };

            var classroom = new Classroom
            {
                ClassroomID = 2,
                RoomNumber = "202"
            };

            var lesson = new Lesson
            {
                LessonID = 2,
                LessonDate = DateTime.Today,
                LessonTime = new TimeSpan(10, 0, 0),
                ClassroomID = classroom.ClassroomID,
                Classroom = classroom,
                StudyGroupID = studyGroup.StudyGroupID,
                StudyGroup = studyGroup
            };

            var factory = CreateFactoryWithSeededData(teacherUser, subject, studyGroup, classroom, lesson);
            var vm = new LessonsVm(factory);

            var studentUser = new User { GroupID = 11 };
            typeof(LessonsVm).GetField("_currentUser", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(vm, studentUser);

            await vm.LoadAllStudentLessons();

            var loadedLesson = vm.AllStudentLessons.First();
            Assert.Equal("Petrov I.", loadedLesson.TeacherFullName);
        }
    }
}
