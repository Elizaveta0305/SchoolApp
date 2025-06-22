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
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class GradeVmTests : IDisposable
    {
        private readonly TestDbContextFactory _dbContextFactory;
        private ApplicationDbContext _currentTestDbContext;
        private IMessenger _messenger;
        private GradeVm _viewModel;

        private Role _studentRole;
        private Role _teacherRole;
        private User _studentUser;
        private User _teacherUser;
        private Group _group9B;
        private Subject _stm32Subject;
        private Subject _scratchSubject;
        private Classroom _classroom101;
        private Classroom _classroom102;
        private StudyGroup _stm32StudyGroup;
        private StudyGroup _scratchStudyGroup;
        private Lesson _stm32Lesson1;
        private Lesson _scratchLesson1;
        private AcademicPerformance _studentStm32Grade;
        private AcademicPerformance _studentScratchGrade;

        public GradeVmTests(MessengerFixture fixture)
        {
            _dbContextFactory = new TestDbContextFactory();
            _messenger = fixture.Messenger;
        }

        private void SetupTest()
        {
            _currentTestDbContext = _dbContextFactory.CreateDbContext();

            _studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            _teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            _group9B = new Group { GroupID = 201, GroupName = "9Б" };
            _stm32Subject = new Subject { SubjectID = 301, SubjectName = "STM32 в среде STM32CubeIDE" };
            _scratchSubject = new Subject { SubjectID = 302, SubjectName = "Scratch" };
            _classroom101 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            _classroom102 = new Classroom { ClassroomID = 2, RoomNumber = "102" };


            _currentTestDbContext.Roles.AddRange(_studentRole, _teacherRole);
            _currentTestDbContext.Groups.Add(_group9B);
            _currentTestDbContext.Subjects.AddRange(_stm32Subject, _scratchSubject);
            _currentTestDbContext.Classrooms.AddRange(_classroom101, _classroom102);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _studentUser = new User
            {
                UserID = 101,
                Username = "student1",
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                RoleID = _studentRole.RoleID,
                GroupID = _group9B.GroupID,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>()
            };

            _teacherUser = new User
            {
                UserID = 102,
                Username = "teacher1",
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                RoleID = _teacherRole.RoleID,
                StudyGroupsAsTeacher = new List<StudyGroup>()
            };

            _currentTestDbContext.Users.AddRange(_studentUser, _teacherUser);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _stm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 401,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _stm32Subject.SubjectID,
                Lessons = new List<Lesson>()
            };
            _scratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 402,
                TeacherID = _teacherUser.UserID,
                GroupID = _group9B.GroupID,
                SubjectID = _scratchSubject.SubjectID,
                Lessons = new List<Lesson>()
            };

            _currentTestDbContext.StudyGroups.AddRange(_stm32StudyGroup, _scratchStudyGroup);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _stm32Lesson1 = new Lesson
            {
                LessonID = 501,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                ClassroomID = _classroom101.ClassroomID,
                LessonDate = new DateTime(2024, 05, 10),
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Введение в STM32CubeIDE",
            };
            _scratchLesson1 = new Lesson
            {
                LessonID = 502,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                ClassroomID = _classroom102.ClassroomID,
                LessonDate = new DateTime(2024, 05, 11),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Создание первого проекта",
            };

            _currentTestDbContext.Lessons.AddRange(_stm32Lesson1, _scratchLesson1);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _studentStm32Grade = new AcademicPerformance
            {
                PerformanceID = 601,
                StudentID = _studentUser.UserID,
                LessonID = _stm32Lesson1.LessonID,
                Grade = "5",
                Attendance = true,
                Comment = "Отличная работа с платой",
            };
            _studentScratchGrade = new AcademicPerformance
            {
                PerformanceID = 602,
                StudentID = _studentUser.UserID,
                LessonID = _scratchLesson1.LessonID,
                Grade = "4",
                Attendance = true,
                Comment = "Креативный подход",
            };

            _currentTestDbContext.AcademicPerformance.AddRange(_studentStm32Grade, _studentScratchGrade);
            _currentTestDbContext.SaveChanges();
            _currentTestDbContext.ChangeTracker.Clear();

            _viewModel = new GradeVm(_dbContextFactory, _messenger);
        }

        public void Dispose()
        {
            _currentTestDbContext?.Dispose();
        }

        private void SeedDatabase(params object[] entities)
        {
            _dbContextFactory.SeedData(_currentTestDbContext, entities);
        }

        private async Task<GradeVm> CreateAndAuthenticateViewModel(User? currentUser = null)
        {
            if (currentUser != null)
            {
                User userFromDb;
                userFromDb = await _currentTestDbContext.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.Group)
                        .ThenInclude(g => g.StudyGroups!)
                            .ThenInclude(sg => sg.Subject)
                    .Include(u => u.AcademicPerformanceAsStudent!)
                        .ThenInclude(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Subject)
                    .Include(u => u.AcademicPerformanceAsStudent!)
                        .ThenInclude(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Teacher)
                    .FirstOrDefaultAsync(u => u.UserID == currentUser.UserID);

                Assert.NotNull(userFromDb);

                _messenger.Send(new UserAuthenticatedMessage(userFromDb));
                await Task.Delay(100);
            }
            return _viewModel;
        }

        [Fact]
        public async Task Receive_WithAuthenticatedStudentUser_LoadsStudentDataAndGrades()
        {
            SetupTest();

            var vm = await CreateAndAuthenticateViewModel(_studentUser);

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
            SetupTest();

            var vm = await CreateAndAuthenticateViewModel(_studentUser);

            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Contains("Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoGrades()
        {
            _currentTestDbContext = _dbContextFactory.CreateDbContext();


            var tempStudentRole = new Role { RoleID = 11, RoleName = "Ученик" };
            var tempTeacherRole = new Role { RoleID = 12, RoleName = "Учитель" };
            var tempGroup = new Group { GroupID = 211, GroupName = "ТестГруппа" };
            var tempStm32Subject = new Subject { SubjectID = 311, SubjectName = "Тест STM32" };
            var tempScratchSubject = new Subject { SubjectID = 312, SubjectName = "Тест Scratch" };
            var tempClassroom1 = new Classroom { ClassroomID = 11, RoomNumber = "201" };
            var tempClassroom2 = new Classroom { ClassroomID = 12, RoomNumber = "202" };

            _currentTestDbContext.Roles.AddRange(tempStudentRole, tempTeacherRole);
            _currentTestDbContext.Groups.Add(tempGroup);
            _currentTestDbContext.Subjects.AddRange(tempStm32Subject, tempScratchSubject);
            _currentTestDbContext.Classrooms.AddRange(tempClassroom1, tempClassroom2);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            var tempTeacherUser = new User
            {
                UserID = 112,
                Username = "tempTeacher",
                FirstName = "Тест",
                LastName = "Учитель",
                MiddleName = "Темп",
                RoleID = tempTeacherRole.RoleID,
                StudyGroupsAsTeacher = new List<StudyGroup>()
            };
            var studentWithoutGrades_Test = new User
            {
                UserID = 113,
                Username = "nogrades",
                FirstName = "Тест",
                LastName = "БезОценок",
                RoleID = tempStudentRole.RoleID,
                GroupID = tempGroup.GroupID,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>()
            };

            _currentTestDbContext.Users.AddRange(tempTeacherUser, studentWithoutGrades_Test);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            var tempStm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 411,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup.GroupID,
                SubjectID = tempStm32Subject.SubjectID,
                Lessons = new List<Lesson>()
            };
            var tempScratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 412,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup.GroupID,
                SubjectID = tempScratchSubject.SubjectID,
                Lessons = new List<Lesson>()
            };
            _currentTestDbContext.StudyGroups.AddRange(tempStm32StudyGroup, tempScratchStudyGroup);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            var tempStm32Lesson = new Lesson
            {
                LessonID = 511,
                StudyGroupID = tempStm32StudyGroup.StudyGroupID,
                ClassroomID = tempClassroom1.ClassroomID,
                LessonDate = new DateTime(2024, 6, 1),
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Тест Урок 1",
            };
            var tempScratchLesson = new Lesson
            {
                LessonID = 512,
                StudyGroupID = tempScratchStudyGroup.StudyGroupID,
                ClassroomID = tempClassroom2.ClassroomID,
                LessonDate = new DateTime(2024, 6, 2),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Тест Урок 2",
            };
            _currentTestDbContext.Lessons.AddRange(tempStm32Lesson, tempScratchLesson);
            await _currentTestDbContext.SaveChangesAsync();
            _currentTestDbContext.ChangeTracker.Clear();

            _viewModel = new GradeVm(_dbContextFactory, _messenger);

            var vm = await CreateAndAuthenticateViewModel(studentWithoutGrades_Test);

            Assert.Equal($"{studentWithoutGrades_Test.LastName} {studentWithoutGrades_Test.FirstName}", vm.StudentFullName);
            Assert.Equal(tempGroup.GroupName, vm.StudentGroupName);
            Assert.Contains("Тест STM32", vm.StudentSubjects);
            Assert.Contains("Тест Scratch", vm.StudentSubjects);
            Assert.Equal(2, vm.StudentSubjects.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Length);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task GradeDisplayModel_CorrectlyMapsData()
        {
            SetupTest();

            var vm = await CreateAndAuthenticateViewModel(_studentUser);

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
            _currentTestDbContext = _dbContextFactory.CreateDbContext();

            var tempStudentRole = new Role { RoleID = 10, RoleName = "Ученик" };
            var studentNoGroup_Test = new User
            {
                UserID = 104,
                Username = "nogroup",
                FirstName = "Тест",
                LastName = "БезГруппы",
                RoleID = tempStudentRole.RoleID,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>()
            };

            SeedDatabase(tempStudentRole, studentNoGroup_Test);

            _viewModel = new GradeVm(_dbContextFactory, _messenger);

            var vm = await CreateAndAuthenticateViewModel(studentNoGroup_Test);

            Assert.Equal($"{studentNoGroup_Test.LastName} {studentNoGroup_Test.FirstName}", vm.StudentFullName);
            Assert.Equal("Группа не определена", vm.StudentGroupName);
            Assert.Equal("Предметы не определены", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades);
        }

        [Fact]
        public async Task Receive_WithNullUser_ResetsProperties()
        {
            SetupTest();

            await CreateAndAuthenticateViewModel(_studentUser);
            Assert.NotEmpty(_viewModel.StudentGrades);

            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.Equal("Неизвестно", _viewModel.StudentFullName);
            Assert.Equal("Неизвестно", _viewModel.StudentGroupName);
            Assert.Equal("Предметы не определены", _viewModel.StudentSubjects);
            Assert.Empty(_viewModel.StudentGrades);
        }

        [Fact]
        public async Task LoadStudentDataAndGrades_HandlesNoLessonsForStudent()
        {
            _currentTestDbContext = _dbContextFactory.CreateDbContext();

            var tempStudentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var tempTeacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var tempGroup9B = new Group { GroupID = 201, GroupName = "9Б" };
            var tempStm32Subject = new Subject { SubjectID = 301, SubjectName = "STM32 в среде STM32CubeIDE" };
            var tempScratchSubject = new Subject { SubjectID = 302, SubjectName = "Scratch" };
            var tempClassroom101 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var tempClassroom102 = new Classroom { ClassroomID = 2, RoomNumber = "102" };

            var studentNoLessons = new User
            {
                UserID = 105,
                Username = "nolessons",
                FirstName = "Тест",
                LastName = "БезУроков",
                RoleID = tempStudentRole.RoleID,
                GroupID = tempGroup9B.GroupID,
                AcademicPerformanceAsStudent = new List<AcademicPerformance>()
            };

            var tempTeacherUser = new User
            {
                UserID = 102,
                Username = "teacher1",
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                RoleID = tempTeacherRole.RoleID,
                StudyGroupsAsTeacher = new List<StudyGroup>()
            };

            var tempStm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 401,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup9B.GroupID,
                SubjectID = tempStm32Subject.SubjectID,
                Lessons = new List<Lesson>()
            };
            var tempScratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 402,
                TeacherID = tempTeacherUser.UserID,
                GroupID = tempGroup9B.GroupID,
                SubjectID = tempScratchSubject.SubjectID,
                Lessons = new List<Lesson>()
            };

            var tempStm32Lesson1 = new Lesson
            {
                LessonID = 501,
                StudyGroupID = tempStm32StudyGroup.StudyGroupID,
                ClassroomID = tempClassroom101.ClassroomID,
                LessonDate = new DateTime(2024, 05, 10),
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Введение в STM32CubeIDE",
            };
            var tempScratchLesson1 = new Lesson
            {
                LessonID = 502,
                StudyGroupID = tempScratchStudyGroup.StudyGroupID,
                ClassroomID = tempClassroom102.ClassroomID,
                LessonDate = new DateTime(2024, 05, 11),
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Создание первого проекта",
            };

            SeedDatabase(
                tempStudentRole,
                tempTeacherRole,
                tempGroup9B,
                tempTeacherUser,
                tempStm32Subject,
                tempScratchSubject,
                tempClassroom101,
                tempClassroom102,
                tempStm32StudyGroup,
                tempScratchStudyGroup,
                tempStm32Lesson1,
                tempScratchLesson1,
                studentNoLessons
            );

            _viewModel = new GradeVm(_dbContextFactory, _messenger);

            var vm = await CreateAndAuthenticateViewModel(studentNoLessons);

            Assert.Equal($"{studentNoLessons.LastName} {studentNoLessons.FirstName}", vm.StudentFullName);
            Assert.Equal(tempGroup9B.GroupName, vm.StudentGroupName);
            Assert.Contains("STM32 в среде STM32CubeIDE", vm.StudentSubjects);
            Assert.Empty(vm.StudentGrades); 
        }
    }
}