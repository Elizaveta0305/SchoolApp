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
    public class HomeVmTests
    {
        private readonly IMessenger _messenger;

        public HomeVmTests()
        {
            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();
        }

        private async Task<HomeVm> CreateViewModel(User currentUser, params object[] entitiesToSeed)
        {
            var dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());

            using (var context = dbContextFactory.CreateDbContext())
            {
                dbContextFactory.SeedData(context, entitiesToSeed);
            }

            var vm = new HomeVm(dbContextFactory);
            if (currentUser != null)
            {
                vm.Receive(new UserAuthenticatedMessage(currentUser));
                await Task.Delay(500);
            }
            return vm;
        }

        [Fact]
        public async Task Receive_WithAuthenticatedUser_LoadsWelcomeMessageAndUpcomingLessons()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var upcomingLesson = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(1), LessonTime = new TimeSpan(9, 0, 0), Topic = "Алгебра", StudyGroup = studyGroupMath, Classroom = classroom1 };

            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1, upcomingLesson);

            Assert.Equal($"Рады вас видеть, {studentUser.FirstName}!", vm.WelcomeMessage);
            Assert.NotEmpty(vm.UpcomingLessons);
            Assert.Single(vm.UpcomingLessons);
            Assert.Equal(upcomingLesson.LessonID, vm.UpcomingLessons.First().LessonId);
            Assert.Equal(subjectMath.SubjectName, vm.UpcomingLessons.First().SubjectName);
            Assert.Equal($"{teacherUser.LastName} {teacherUser.FirstName[0]}.{teacherUser.MiddleName[0]}.", vm.UpcomingLessons.First().TeacherFullName);
        }

        [Fact]
        public async Task Receive_WithNullUser_ClearsAllData()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var upcomingLesson = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(1), LessonTime = new TimeSpan(9, 0, 0), Topic = "Алгебра", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var grade = new AcademicPerformance { PerformanceID = 1, StudentID = studentUser.UserID, LessonID = upcomingLesson.LessonID, Grade = "5", Attendance = true, Student = studentUser, Lesson = upcomingLesson };


            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1, upcomingLesson, grade);

            await Task.Delay(200);

            Assert.Equal($"Рады вас видеть, {studentUser.FirstName}!", vm.WelcomeMessage);
            Assert.NotEmpty(vm.UpcomingLessons);
            Assert.True(vm.AbsencesCount >= 0);
            Assert.True(vm.SubjectsCount > 0);
            Assert.True(vm.AverageGradeValue > 0.0);

            vm.Receive(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.Equal("Добро пожаловать!", vm.WelcomeMessage);
            Assert.Empty(vm.UpcomingLessons);
            Assert.Equal(0, vm.AbsencesCount);
            Assert.Equal("0 / 30", vm.AbsencesDisplayText);
            Assert.Equal(0, vm.SubjectsCount);
            Assert.Equal(0.0, vm.AverageGradeValue);
            Assert.Equal("Н/Д", vm.AverageGradeDisplayText);
            Assert.Equal("Неизвестно", vm.AcademicYear);
        }

        [Fact]
        public async Task LoadUpcomingLessonsInternal_LoadsOnlyFutureLessonsForCurrentUserGroup()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };

            var upcomingLesson1 = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(1), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 1", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var upcomingLesson2 = new Lesson { LessonID = 2, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(2), LessonTime = new TimeSpan(10, 0, 0), Topic = "Урок 2", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var pastLesson = new Lesson { LessonID = 3, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-1), LessonTime = new TimeSpan(11, 0, 0), Topic = "Прошлый урок", StudyGroup = studyGroupMath, Classroom = classroom1 };

            var otherGroup = new Group { GroupID = 102, GroupName = "10А" };
            var otherStudyGroup = new StudyGroup { StudyGroupID = 2, TeacherID = teacherUser.UserID, GroupID = otherGroup.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = otherGroup, Subject = subjectMath };
            var otherGroupLesson = new Lesson { LessonID = 4, StudyGroupID = otherStudyGroup.StudyGroupID, LessonDate = DateTime.Today.AddDays(1), LessonTime = new TimeSpan(14, 0, 0), Topic = "Урок для другой группы", StudyGroup = otherStudyGroup, Classroom = classroom1 };


            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1,
                upcomingLesson1, upcomingLesson2, pastLesson, otherGroup, otherStudyGroup, otherGroupLesson);

            Assert.Equal(2, vm.UpcomingLessons.Count);
            Assert.Contains(vm.UpcomingLessons, l => l.LessonId == upcomingLesson1.LessonID);
            Assert.Contains(vm.UpcomingLessons, l => l.LessonId == upcomingLesson2.LessonID);
            Assert.DoesNotContain(vm.UpcomingLessons, l => l.LessonId == pastLesson.LessonID);
            Assert.DoesNotContain(vm.UpcomingLessons, l => l.LessonId == otherGroupLesson.LessonID);
        }

        [Fact]
        public async Task LoadAnalyticsData_CalculatesAbsencesCorrectly()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var lesson1 = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-5), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 1", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var lesson2 = new Lesson { LessonID = 2, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-4), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 2", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var lesson3 = new Lesson { LessonID = 3, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-3), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 3", StudyGroup = studyGroupMath, Classroom = classroom1 };

            var absence1 = new AcademicPerformance { PerformanceID = 1, StudentID = studentUser.UserID, LessonID = lesson1.LessonID, Attendance = false, Student = studentUser, Lesson = lesson1 };
            var absence2 = new AcademicPerformance { PerformanceID = 2, StudentID = studentUser.UserID, LessonID = lesson2.LessonID, Attendance = false, Student = studentUser, Lesson = lesson2 };
            var presence = new AcademicPerformance { PerformanceID = 3, StudentID = studentUser.UserID, LessonID = lesson3.LessonID, Attendance = true, Student = studentUser, Lesson = lesson3 };

            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1,
                lesson1, lesson2, lesson3, absence1, absence2, presence);

            Assert.Equal(2, vm.AbsencesCount);
            Assert.Equal("2 / 30", vm.AbsencesDisplayText);
        }

        [Fact]
        public async Task LoadAnalyticsData_CalculatesAverageGradeCorrectly()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "Мария", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var lesson1 = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-5), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 1", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var lesson2 = new Lesson { LessonID = 2, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-4), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 2", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var lesson3 = new Lesson { LessonID = 3, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-3), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 3", StudyGroup = studyGroupMath, Classroom = classroom1 };
            var lesson4 = new Lesson { LessonID = 4, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-2), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 4", StudyGroup = studyGroupMath, Classroom = classroom1 };


            var grade1 = new AcademicPerformance { PerformanceID = 1, StudentID = studentUser.UserID, LessonID = lesson1.LessonID, Grade = "5", Attendance = true, Student = studentUser, Lesson = lesson1 };
            var grade2 = new AcademicPerformance { PerformanceID = 2, StudentID = studentUser.UserID, LessonID = lesson2.LessonID, Grade = "4", Attendance = true, Student = studentUser, Lesson = lesson2 };
            var grade3 = new AcademicPerformance { PerformanceID = 3, StudentID = studentUser.UserID, LessonID = lesson3.LessonID, Grade = "3", Attendance = true, Student = studentUser, Lesson = lesson3 };
            var grade4_nonNumeric = new AcademicPerformance { PerformanceID = 4, StudentID = studentUser.UserID, LessonID = lesson4.LessonID, Grade = "Н/А", Attendance = true, Student = studentUser, Lesson = lesson4 };

            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1,
                lesson1, lesson2, lesson3, lesson4,
                grade1, grade2, grade3, grade4_nonNumeric);

            Assert.Equal(4.0, vm.AverageGradeValue);
            Assert.Equal("4.00", vm.AverageGradeDisplayText);
        }

        [Fact]
        public async Task LoadAnalyticsData_HandlesNoGrades()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };
            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var classroom1 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            var lesson1 = new Lesson { LessonID = 1, StudyGroupID = studyGroupMath.StudyGroupID, LessonDate = DateTime.Today.AddDays(-5), LessonTime = new TimeSpan(9, 0, 0), Topic = "Урок 1", StudyGroup = studyGroupMath, Classroom = classroom1 };


            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser, subjectMath, studyGroupMath, classroom1,
                lesson1);

            Assert.Equal(0.0, vm.AverageGradeValue);
            Assert.Equal("Н/Д", vm.AverageGradeDisplayText);
        }

        [Fact]
        public async Task LoadAnalyticsData_CalculatesSubjectsCountCorrectly()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var group9B = new Group { GroupID = 101, GroupName = "9Б" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = group9B.GroupID, Group = group9B };
            var teacherRole = new Role { RoleID = 2, RoleName = "Учитель" };
            var teacherUser = new User { UserID = 2, Username = "teacher1", FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна", RoleID = teacherRole.RoleID, Role = teacherRole };

            var subjectMath = new Subject { SubjectID = 1, SubjectName = "Математика" };
            var subjectPhysics = new Subject { SubjectID = 2, SubjectName = "Физика" };
            var subjectChemistry = new Subject { SubjectID = 3, SubjectName = "Химия" };

            var studyGroupMath = new StudyGroup { StudyGroupID = 1, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectMath.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectMath };
            var studyGroupPhysics = new StudyGroup { StudyGroupID = 2, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectPhysics.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectPhysics };
            var studyGroupChemistry = new StudyGroup { StudyGroupID = 3, TeacherID = teacherUser.UserID, GroupID = group9B.GroupID, SubjectID = subjectChemistry.SubjectID, Teacher = teacherUser, Group = group9B, Subject = subjectChemistry };

            var vm = await CreateViewModel(studentUser,
                studentRole, teacherRole, group9B, studentUser, teacherUser,
                subjectMath, subjectPhysics, subjectChemistry,
                studyGroupMath, studyGroupPhysics, studyGroupChemistry);

            Assert.Equal(3, vm.SubjectsCount);
        }

        [Fact]
        public async Task LoadAnalyticsData_HandlesUserWithoutGroup()
        {
            var studentRole = new Role { RoleID = 1, RoleName = "Ученик" };
            var studentUser = new User { UserID = 1, Username = "student1", FirstName = "Иван", LastName = "Петров", RoleID = studentRole.RoleID, Role = studentRole, GroupID = null };

            var vm = await CreateViewModel(studentUser, studentRole, studentUser);

            Assert.Equal(0, vm.AbsencesCount);
            Assert.Equal("0 / 30", vm.AbsencesDisplayText);
            Assert.Equal(0, vm.SubjectsCount);
            Assert.Equal(0.0, vm.AverageGradeValue);
            Assert.Equal("Н/Д", vm.AverageGradeDisplayText);
            Assert.Equal("Неизвестно", vm.AcademicYear);
        }
    }
}