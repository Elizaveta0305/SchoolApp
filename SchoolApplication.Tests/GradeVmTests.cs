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
using System.Collections.ObjectModel;

namespace SchoolApplication.Tests
{
    public class GradeVmTests
    {
        private readonly IMessenger _messenger;
        private readonly TestDbContextFactory _dbContextFactory;
        private readonly Role _studentRole;
        private readonly Role _teacherRole;
        private readonly User _studentUser;
        private readonly User _teacherUser;
        private readonly Group _group9B;
        private readonly Subject _stm32Subject;
        private readonly Subject _scratchSubject;
        private readonly StudyGroup _stm32StudyGroup;
        private readonly StudyGroup _scratchStudyGroup;
        private readonly Lesson _stm32Lesson1;
        private readonly Lesson _scratchLesson1;
        private readonly AcademicPerformance _studentStm32Grade;
        private readonly AcademicPerformance _studentScratchGrade;

        public GradeVmTests()
        {
            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();

            _dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());

            _studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            _teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };

            _group9B = new Group { GroupID = 201, GroupName = "9Б", Users = new List<User>(), StudyGroups = new List<StudyGroup>() };

            _studentUser = new User
            {
                UserID = 101,
                Username = "student1",
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                RoleID = _studentRole.RoleID,
                Role = _studentRole,
                GroupID = _group9B.GroupID,
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
                Teacher = _teacherUser,
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
                StudyGroup = _stm32StudyGroup
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

            _group9B.Users.Add(_studentUser);
            _group9B.StudyGroups.Add(_stm32StudyGroup);
            _group9B.StudyGroups.Add(_scratchStudyGroup);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context,
                    _studentRole,
                    _teacherRole,
                    _teacherUser,
                    _studentUser,
                    _group9B,
                    _stm32Subject,
                    _scratchSubject,
                    _stm32StudyGroup,
                    _scratchStudyGroup,
                    _stm32Lesson1,
                    _scratchLesson1,
                    _studentStm32Grade,
                    _studentScratchGrade
                );
            }
        }

        private async Task<GradeVm> CreateViewModel(User? currentUser = null)
        {
            var vm = new GradeVm(_dbContextFactory);
            if (currentUser != null)
            {
                vm.Receive(new UserAuthenticatedMessage(currentUser));
                await Task.Delay(500);
            }
            return vm;
        }

        [Fact]
        public async Task Receive_WithAuthenticatedStudentUser_LoadsStudentDataAndGrades()
        {
            var vm = await CreateViewModel();
            Assert.Equal("Неизвестно", vm.StudentFullName);
            Assert.Equal("Неизвестно", vm.StudentGroupName);
            Assert.Equal("Загрузка...", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);

            vm.Receive(new UserAuthenticatedMessage(_studentUser));
            await Task.Delay(500);

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
        public async Task Receive_WithNullUser_ClearsAllData()
        {
            var vm = await CreateViewModel(_studentUser);
            await Task.Delay(100);

            Assert.NotEmpty(vm.StudentGrades);
            Assert.NotNull(vm.StudentFullName);

            vm.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.Equal("Неизвестно", vm.StudentFullName);
            Assert.Equal("Неизвестно", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_LoadsCorrectSubjects()
        {
            var vm = await CreateViewModel(_studentUser);
            await Task.Delay(100);

            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(", ").Length);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoGrades()
        {
            var tempStudentRole = new Role { RoleID = 11, RoleName = "Ученик" };
            var tempTeacherRole = new Role { RoleID = 12, RoleName = "Учитель" };
            var tempTeacherUser = new User { UserID = 112, Username = "tempTeacher", FirstName = "Тест", LastName = "Учитель", MiddleName = "Темп", RoleID = tempTeacherRole.RoleID, Role = tempTeacherRole };
            var tempGroup = new Group { GroupID = 211, GroupName = "ТестГруппа", Users = new List<User>(), StudyGroups = new List<StudyGroup>() };
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
            tempGroup.StudyGroups.Add(tempStm32StudyGroup);
            tempGroup.StudyGroups.Add(tempScratchStudyGroup);

            var studentWithoutGrades_Test = new User { UserID = 113, Username = "nogrades", FirstName = "Тест", LastName = "БезОценок", RoleID = tempStudentRole.RoleID, GroupID = tempGroup.GroupID, Group = tempGroup, Role = tempStudentRole };
            tempGroup.Users.Add(studentWithoutGrades_Test);

            var testDbFactory = new TestDbContextFactory(Guid.NewGuid().ToString());
            using (var testContext = testDbFactory.CreateDbContext())
            {
                testDbFactory.SeedData(testContext,
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
            }

            var vm = new GradeVm(testDbFactory);
            vm.Receive(new UserAuthenticatedMessage(studentWithoutGrades_Test));
            await Task.Delay(500);

            Assert.Equal($"{studentWithoutGrades_Test.LastName} {studentWithoutGrades_Test.FirstName}", vm.StudentFullName);
            Assert.Equal(tempGroup.GroupName, vm.StudentGroupName);
            Assert.Contains("Тест STM32", vm.StudentSubjects);
            Assert.Contains("Тест Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(", ").Length);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task GradeDisplayModel_CorrectlyMapsData()
        {
            var vm = await CreateViewModel(_studentUser);
            await Task.Delay(100);

            var stm32DisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentStm32Grade.PerformanceID);
            Assert.NotNull(stm32DisplayGrade);
            Assert.Equal(_stm32Subject.SubjectName, stm32DisplayGrade.SubjectName);
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
            var studentNoGroup = new User { UserID = 104, Username = "nogroup", FirstName = "Тест", LastName = "БезГруппы", RoleID = _studentRole.RoleID, Role = _studentRole };

            var testDbFactory = new TestDbContextFactory(Guid.NewGuid().ToString());
            using (var context = testDbFactory.CreateDbContext())
            {
                testDbFactory.SeedData(context,
                    _studentRole,
                    studentNoGroup
                );
            }

            var vm = new GradeVm(testDbFactory);
            vm.Receive(new UserAuthenticatedMessage(studentNoGroup));
            await Task.Delay(500);

            Assert.Equal($"{studentNoGroup.LastName} {studentNoGroup.FirstName}", vm.StudentFullName);
            Assert.Equal("Группа не определена", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }
    }
}