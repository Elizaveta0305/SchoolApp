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

            _studentRole = new Role { RoleID = 3, RoleName = "Ученик" };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };

            _studentUser = new User { UserID = 201, Username = "student1", FirstName = "Дмитрий", LastName = "Смирнов", RoleID = _studentRole.RoleID, Role = _studentRole };
            _teacherUser = new User { UserID = 101, Username = "teacher1", FirstName = "Иван", LastName = "Иванов", RoleID = teacherRole.RoleID, Role = teacherRole };

            _group9B = new Group { GroupID = 1, GroupName = "9Б", Users = new List<User>(), StudyGroups = new List<StudyGroup>() };

            _stm32Subject = new Subject { SubjectID = 1, SubjectName = "STM32 в среде STM32CubeIDE" };
            _scratchSubject = new Subject { SubjectID = 2, SubjectName = "Scratch" };

            _stm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 1001,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _stm32Subject.SubjectID,
                Teacher = _teacherUser,
                Group = _group9B,
                Subject = _stm32Subject
            };
            _scratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 1002,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _scratchSubject.SubjectID,
                Teacher = _teacherUser,
                Group = _group9B,
                Subject = _scratchSubject
            };

            _stm32Lesson1 = new Lesson
            {
                LessonID = 301,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                LessonDate = new DateTime(2024, 05, 10),
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Введение в STM32CubeIDE",
                StudyGroup = _stm32StudyGroup
            };
            _scratchLesson1 = new Lesson
            {
                LessonID = 302,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                LessonDate = new DateTime(2024, 05, 11),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Создание первого проекта",
                StudyGroup = _scratchStudyGroup
            };

            _studentStm32Grade = new AcademicPerformance
            {
                PerformanceID = 401,
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
                PerformanceID = 402,
                StudentID = _studentUser.UserID,
                LessonID = _scratchLesson1.LessonID,
                Grade = "4",
                Attendance = true,
                Comment = "Креативный подход",
                Student = _studentUser,
                Lesson = _scratchLesson1
            };

            _studentUser.Group = _group9B;
            _studentUser.GroupID = _group9B.GroupID;
            _group9B.Users.Add(_studentUser);
            _group9B.StudyGroups.Add(_stm32StudyGroup);
            _group9B.StudyGroups.Add(_scratchStudyGroup);
        }

        private async Task<GradeVm> CreateViewModel(User? currentUser = null)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context,
                    _studentRole,
                    _teacherUser.Role,
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
            var vm = new GradeVm(_dbContextFactory);
            if (currentUser != null)
            {
                vm.Receive(new UserAuthenticatedMessage(currentUser));
                await Task.Delay(50);
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
            await Task.Delay(100);

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
            Assert.NotEmpty(vm.StudentGrades);
            Assert.NotNull(vm.StudentFullName);

            vm.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(50);

            Assert.Equal("Неизвестно", vm.StudentFullName);
            Assert.Equal("Неизвестно", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_LoadsCorrectSubjects()
        {
            var vm = await CreateViewModel(_studentUser);
            await vm.LoadStudentDataAndGrades();
            await Task.Delay(100);

            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(", ").Length);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoGrades()
        {
            var studentWithoutGrades = new User { UserID = 203, Username = "nogrades", FirstName = "Тест", LastName = "БезОценок", RoleID = _studentRole.RoleID, GroupID = _group9B.GroupID, Group = _group9B, Role = _studentRole };
            _group9B.Users.Add(studentWithoutGrades);


            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context,
                    _studentRole,
                    _teacherUser.Role,
                    _teacherUser,
                    _group9B,
                    _stm32Subject,
                    _scratchSubject,
                    _stm32StudyGroup,
                    _scratchStudyGroup,
                    _stm32Lesson1,
                    _scratchLesson1,
                    studentWithoutGrades
                );
            }

            var vm = new GradeVm(_dbContextFactory);
            vm.Receive(new UserAuthenticatedMessage(studentWithoutGrades));
            await Task.Delay(100);

            Assert.Equal($"{studentWithoutGrades.LastName} {studentWithoutGrades.FirstName}", vm.StudentFullName);
            Assert.Equal(_group9B.GroupName, vm.StudentGroupName);
            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(", ").Length);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task GradeDisplayModel_CorrectlyMapsData()
        {
            var vm = await CreateViewModel(_studentUser);
            await vm.LoadStudentDataAndGrades();
            await Task.Delay(100);

            var stm32DisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentStm32Grade.PerformanceID);
            Assert.NotNull(stm32DisplayGrade);
            Assert.Equal(_stm32Subject.SubjectName, stm32DisplayGrade.SubjectName);
            Assert.Contains(_stm32StudyGroup.Teacher.LastName, stm32DisplayGrade.TeacherFullName);
            Assert.Equal(DateOnly.FromDateTime(_stm32Lesson1.LessonDate), stm32DisplayGrade.LessonDate);
            Assert.Equal(_stm32Lesson1.LessonTime, stm32DisplayGrade.LessonTime);
            Assert.Equal(_studentStm32Grade.Grade, stm32DisplayGrade.GradeValue);
            Assert.Equal(_studentStm32Grade.Attendance, stm32DisplayGrade.AttendanceMark);
            Assert.Equal(_studentStm32Grade.Comment, stm32DisplayGrade.Comment);

            var scratchDisplayGrade = vm.StudentGrades.FirstOrDefault(g => g.PerformanceID == _studentScratchGrade.PerformanceID);
            Assert.NotNull(scratchDisplayGrade);
            Assert.Equal(_scratchSubject.SubjectName, scratchDisplayGrade.SubjectName);
            Assert.Contains(_scratchStudyGroup.Teacher.LastName, scratchDisplayGrade.TeacherFullName);
            Assert.Equal(DateOnly.FromDateTime(_scratchLesson1.LessonDate), scratchDisplayGrade.LessonDate);
            Assert.Equal(_scratchLesson1.LessonTime, scratchDisplayGrade.LessonTime);
            Assert.Equal(_studentScratchGrade.Grade, scratchDisplayGrade.GradeValue);
            Assert.Equal(_studentScratchGrade.Attendance, scratchDisplayGrade.AttendanceMark);
            Assert.Equal(_studentScratchGrade.Comment, scratchDisplayGrade.Comment);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesMissingGroupOrSubjects()
        {
            var studentNoGroup = new User { UserID = 204, Username = "nogroup", FirstName = "Тест", LastName = "БезГруппы", RoleID = _studentRole.RoleID, Role = _studentRole };

            using (var context = _dbContextFactory.CreateDbContext())
            {
                _dbContextFactory.SeedData(context,
                    _studentRole,
                    studentNoGroup
                );
            }

            var vm = new GradeVm(_dbContextFactory);
            vm.Receive(new UserAuthenticatedMessage(studentNoGroup));
            await Task.Delay(100);

            Assert.Equal($"{studentNoGroup.LastName} {studentNoGroup.FirstName}", vm.StudentFullName);
            Assert.Equal("Группа не определена", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }
    }
}