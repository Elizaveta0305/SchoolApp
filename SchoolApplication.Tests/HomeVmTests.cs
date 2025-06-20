using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.Models.DisplayModels;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace SchoolApplication.Tests
{
    public class HomeVmTests
    {
        private readonly User _teacherUser;
        private readonly User _studentUser;
        private readonly Group _group9A;
        private readonly Group _group9B;
        private readonly Subject _mathSubject;
        private readonly Subject _physicsSubject;
        private readonly Subject _stm32Subject;
        private readonly Subject _scratchSubject;
        private readonly StudyGroup _stm32StudyGroup;
        private readonly StudyGroup _scratchStudyGroup;
        private readonly Classroom _classroom101;
        private readonly Classroom _classroom102;
        private readonly Role _teacherRole;
        private readonly Role _studentRole;
        private readonly Lesson _stm32Lesson1Past;
        private readonly Lesson _stm32Lesson2Future;
        private readonly Lesson _scratchLesson1Past;
        private readonly Lesson _scratchLesson2Future;

        public HomeVmTests()
        {
            _teacherRole = new Role { RoleID = 1, RoleName = "Teacher" };
            _studentRole = new Role { RoleID = 2, RoleName = "Student" };

            _group9A = new Group { GroupID = 1, GroupName = "9A" };
            _group9B = new Group { GroupID = 2, GroupName = "9B" };

            _mathSubject = new Subject { SubjectID = 1, SubjectName = "Математика" };
            _physicsSubject = new Subject { SubjectID = 2, SubjectName = "Физика" };
            _stm32Subject = new Subject { SubjectID = 3, SubjectName = "Микроконтроллеры STM32" };
            _scratchSubject = new Subject { SubjectID = 4, SubjectName = "Программирование Scratch" };

            _teacherUser = new User
            {
                UserID = 1,
                Username = "teacher1",
                PasswordHash = "hashed_password",
                FirstName = "Иван",
                LastName = "Петров",
                RoleID = _teacherRole.RoleID,
                Role = _teacherRole
            };

            _studentUser = new User
            {
                UserID = 2,
                Username = "student1",
                PasswordHash = "hashed_password",
                FirstName = "Анна",
                LastName = "Сидорова",
                RoleID = _studentRole.RoleID,
                GroupID = _group9A.GroupID,
                Group = _group9A,
                Role = _studentRole
            };

            _classroom101 = new Classroom { ClassroomID = 1, RoomNumber = "101" };
            _classroom102 = new Classroom { ClassroomID = 2, RoomNumber = "102" };

            _stm32StudyGroup = new StudyGroup
            {
                StudyGroupID = 1,
                TeacherID = _teacherUser.UserID,
                SubjectID = _stm32Subject.SubjectID,
                GroupID = _group9A.GroupID,
                Teacher = _teacherUser,
                Subject = _stm32Subject,
                Group = _group9A
            };

            _scratchStudyGroup = new StudyGroup
            {
                StudyGroupID = 2,
                TeacherID = _teacherUser.UserID,
                SubjectID = _scratchSubject.SubjectID,
                GroupID = _group9A.GroupID,
                Teacher = _teacherUser,
                Subject = _scratchSubject,
                Group = _group9A
            };

            _stm32Lesson1Past = new Lesson
            {
                LessonID = 1,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(-5).Date,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Введение в STM32",
                StudyGroup = _stm32StudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };

            _stm32Lesson2Future = new Lesson
            {
                LessonID = 2,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(5).Date,
                LessonTime = new TimeSpan(11, 0, 0),
                Topic = "GPIO на STM32",
                StudyGroup = _stm32StudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };

            _scratchLesson1Past = new Lesson
            {
                LessonID = 3,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(-10).Date,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Основы Scratch",
                StudyGroup = _scratchStudyGroup,
                Classroom = _classroom102,
                ClassroomID = _classroom102.ClassroomID
            };

            _scratchLesson2Future = new Lesson
            {
                LessonID = 4,
                StudyGroupID = _scratchStudyGroup.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(10).Date,
                LessonTime = new TimeSpan(14, 0, 0),
                Topic = "Создание анимации в Scratch",
                StudyGroup = _scratchStudyGroup,
                Classroom = _classroom102,
                ClassroomID = _classroom102.ClassroomID
            };
        }

        private async Task<HomeVm> CreateViewModel(User user, params object[] entitiesToSeed)
        {
            var dbContextFactory = new TestDbContextFactory(Guid.NewGuid().ToString());

            using (var context = dbContextFactory.CreateDbContext())
            {
                dbContextFactory.SeedData(context, entitiesToSeed);
                await context.SaveChangesAsync();
            }

            var vm = new HomeVm(dbContextFactory);
            vm.Receive(new UserAuthenticatedMessage(user));
            await Task.Delay(200);
            return vm;
        }

        [Fact]
        public async Task LoadUpcomingLessonsInternal_DisplaysUpcomingLessonsCorrectly()
        {
            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUser,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                _stm32Lesson1Past, _stm32Lesson2Future, _scratchLesson1Past, _scratchLesson2Future
            };

            var vm = await CreateViewModel(_studentUser, entities.ToArray());

            Assert.NotNull(vm.UpcomingLessons);
            Assert.Equal(2, vm.UpcomingLessons.Count);

            var firstLesson = vm.UpcomingLessons.FirstOrDefault(l => l.LessonId == _stm32Lesson2Future.LessonID);
            Assert.NotNull(firstLesson);
            Assert.Equal(_stm32StudyGroup.Subject.SubjectName, firstLesson.SubjectName);
            Assert.Equal(_stm32Lesson2Future.LessonDate.ToShortDateString(), firstLesson.FormattedLessonDate);
            Assert.Equal(_stm32Lesson2Future.LessonTime.ToString(@"hh\:mm"), firstLesson.FormattedLessonTime);
            Assert.Equal(_classroom101.RoomNumber, firstLesson.RoomNumber);
            Assert.Equal($"{_stm32StudyGroup.Teacher.LastName} {_stm32StudyGroup.Teacher.FirstName.Substring(0, 1)}.", firstLesson.TeacherFullName); // ИЗМЕНЕНО: ожидаем сокращенное имя

            var secondLesson = vm.UpcomingLessons.FirstOrDefault(l => l.LessonId == _scratchLesson2Future.LessonID);
            Assert.NotNull(secondLesson);
            Assert.Equal(_scratchStudyGroup.Subject.SubjectName, secondLesson.SubjectName);
            Assert.Equal(_scratchLesson2Future.LessonDate.ToShortDateString(), secondLesson.FormattedLessonDate);
            Assert.Equal(_scratchLesson2Future.LessonTime.ToString(@"hh\:mm"), secondLesson.FormattedLessonTime);
            Assert.Equal(_classroom102.RoomNumber, secondLesson.RoomNumber);
            Assert.Equal($"{_scratchStudyGroup.Teacher.LastName} {_scratchStudyGroup.Teacher.FirstName.Substring(0, 1)}.", secondLesson.TeacherFullName); // ИЗМЕНЕНО: ожидаем сокращенное имя
        }

        [Fact]
        public async Task LoadUpcomingLessonsInternal_HandlesNoUpcomingLessons()
        {
            var studentOnlyPastLessons = new User { UserID = 205, Username = "pastonly", FirstName = "Тест", LastName = "БезБудущих", RoleID = _studentRole.RoleID, GroupID = _group9A.GroupID, Group = _group9A, Role = _studentRole };

            var studyGroupPast1 = new StudyGroup
            {
                StudyGroupID = 101,
                TeacherID = _teacherUser.UserID,
                SubjectID = _stm32Subject.SubjectID,
                GroupID = _group9A.GroupID,
                Teacher = _teacherUser,
                Subject = _stm32Subject,
                Group = _group9A
            };
            var lessonPast1 = new Lesson
            {
                LessonID = 1001,
                StudyGroupID = studyGroupPast1.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(-5).Date,
                LessonTime = new TimeSpan(10, 0, 0),
                Topic = "Старый урок 1",
                StudyGroup = studyGroupPast1,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };

            var studyGroupPast2 = new StudyGroup
            {
                StudyGroupID = 102,
                TeacherID = _teacherUser.UserID,
                SubjectID = _scratchSubject.SubjectID,
                GroupID = _group9A.GroupID,
                Teacher = _teacherUser,
                Subject = _scratchSubject,
                Group = _group9A
            };
            var lessonPast2 = new Lesson
            {
                LessonID = 1002,
                StudyGroupID = studyGroupPast2.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(-10).Date,
                LessonTime = new TimeSpan(9, 0, 0),
                Topic = "Старый урок 2",
                StudyGroup = studyGroupPast2,
                Classroom = _classroom102,
                ClassroomID = _classroom102.ClassroomID
            };

            var entities = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, studentOnlyPastLessons,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _classroom101, _classroom102,
                studyGroupPast1, studyGroupPast2,
                lessonPast1, lessonPast2
            };

            var vm = await CreateViewModel(studentOnlyPastLessons, entities.ToArray());

            Assert.Empty(vm.UpcomingLessons);
        }

        [Fact]
        public async Task LoadAnalyticsData_CalculatesAbsencesCorrectly()
        {
            var entitiesScenario1 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUser,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                _stm32Lesson1Past, _stm32Lesson2Future, _scratchLesson1Past, _scratchLesson2Future,
                new AcademicPerformance { PerformanceID = 1, StudentID = _studentUser.UserID, LessonID = _stm32Lesson1Past.LessonID, Grade = "5", Attendance = true, Student = _studentUser, Lesson = _stm32Lesson1Past },
                new AcademicPerformance { PerformanceID = 2, StudentID = _studentUser.UserID, LessonID = _scratchLesson1Past.LessonID, Grade = "4", Attendance = false, Student = _studentUser, Lesson = _scratchLesson1Past }
            };
            var vm = await CreateViewModel(_studentUser, entitiesScenario1.ToArray());
            Assert.Equal(1, vm.AbsencesCount);
            Assert.Equal("1 / 30", vm.AbsencesDisplayText);

            var studentForAbsences2 = new User { UserID = 206, Username = "studentForAbsences2", FirstName = "Тест", LastName = "Отсутствия2", RoleID = _studentRole.RoleID, GroupID = _group9A.GroupID, Group = _group9A, Role = _studentRole };
            var lessonForSecondAbsence = new Lesson
            {
                LessonID = 5001,
                StudyGroupID = _stm32StudyGroup.StudyGroupID,
                LessonDate = DateTime.Now.AddDays(-7).Date,
                LessonTime = new TimeSpan(12, 0, 0),
                Topic = "Дополнительный урок для пропуска",
                StudyGroup = _stm32StudyGroup,
                Classroom = _classroom101,
                ClassroomID = _classroom101.ClassroomID
            };
            var secondAbsence = new AcademicPerformance
            {
                PerformanceID = 5002,
                StudentID = studentForAbsences2.UserID,
                LessonID = lessonForSecondAbsence.LessonID,
                Grade = null,
                Attendance = false,
                Comment = "Второй пропуск",
                Student = studentForAbsences2,
                Lesson = lessonForSecondAbsence
            };
            var entitiesScenario2 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, studentForAbsences2,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                lessonForSecondAbsence, secondAbsence
            };
            var vmWithMoreAbsences = await CreateViewModel(studentForAbsences2, entitiesScenario2.ToArray());
            Assert.Equal(1, vmWithMoreAbsences.AbsencesCount);
            Assert.Equal("1 / 30", vmWithMoreAbsences.AbsencesDisplayText);

            var studentManyAbsences = new User { UserID = 207, Username = "manyabsences", FirstName = "Тест", LastName = "МногоПропусков", RoleID = _studentRole.RoleID, GroupID = _group9A.GroupID, Group = _group9A, Role = _studentRole };
            var entitiesScenario3 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, studentManyAbsences,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102
            };

            for (int i = 0; i < 35; i++)
            {
                var lessonForAbsence = new Lesson
                {
                    LessonID = 6000 + i,
                    StudyGroupID = _stm32StudyGroup.StudyGroupID,
                    LessonDate = DateTime.Now.AddDays(-10 - i).Date,
                    LessonTime = new TimeSpan(10, 0, 0),
                    Topic = $"Прошлый урок для пропуска {i}",
                    StudyGroup = _stm32StudyGroup,
                    Classroom = _classroom101,
                    ClassroomID = _classroom101.ClassroomID
                };
                entitiesScenario3.Add(lessonForAbsence);

                entitiesScenario3.Add(new AcademicPerformance
                {
                    PerformanceID = 7000 + i,
                    StudentID = studentManyAbsences.UserID,
                    LessonID = lessonForAbsence.LessonID,
                    Grade = null,
                    Attendance = false,
                    Comment = $"Пропуск {i}",
                    Student = studentManyAbsences,
                    Lesson = lessonForAbsence
                });
            }

            var vmWithManyAbsences = await CreateViewModel(studentManyAbsences, entitiesScenario3.ToArray());
            Assert.Equal((int)HomeVm.MaxAbsencesValue, vmWithManyAbsences.AbsencesCount);
            Assert.Equal("35 / 30", vmWithManyAbsences.AbsencesDisplayText);
        }

        [Fact]
        public async Task LoadAnalyticsData_CalculatesAverageGradeCorrectly()
        {
            var entitiesScenario1 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, _studentUser,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                _stm32Lesson1Past, _stm32Lesson2Future, _scratchLesson1Past, _scratchLesson2Future,
                new AcademicPerformance { PerformanceID = 100, StudentID = _studentUser.UserID, LessonID = _stm32Lesson1Past.LessonID, Grade = "5", Attendance = true, Student = _studentUser, Lesson = _stm32Lesson1Past },
                new AcademicPerformance { PerformanceID = 101, StudentID = _studentUser.UserID, LessonID = _scratchLesson1Past.LessonID, Grade = "4", Attendance = false, Student = _studentUser, Lesson = _scratchLesson1Past }
            };
            var vm = await CreateViewModel(_studentUser, entitiesScenario1.ToArray());
            Assert.Equal(4.50, vm.AverageGradeValue, 2);
            Assert.Equal("4.50", vm.AverageGradeDisplayText);

            var studentNoGrades = new User { UserID = 207, Username = "nogradesavg", FirstName = "Тест", LastName = "БезОценокAvg", RoleID = _studentRole.RoleID, GroupID = _group9A.GroupID, Group = _group9A, Role = _studentRole };
            var entitiesScenario2 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, studentNoGrades,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                _stm32Lesson1Past, _stm32Lesson2Future, _scratchLesson1Past, _scratchLesson2Future
            };
            var vmNoGrades = await CreateViewModel(studentNoGrades, entitiesScenario2.ToArray());
            Assert.Equal(0.0, vmNoGrades.AverageGradeValue);
            Assert.Equal("0.00", vmNoGrades.AverageGradeDisplayText);

            var studentInvalidGrades = new User { UserID = 208, Username = "invalidgrades", FirstName = "Тест", LastName = "Некорректные", RoleID = _studentRole.RoleID, GroupID = _group9A.GroupID, Group = _group9A, Role = _studentRole };
            var invalidGrade1 = new AcademicPerformance
            {
                PerformanceID = 6001,
                StudentID = studentInvalidGrades.UserID,
                LessonID = _stm32Lesson1Past.LessonID,
                Grade = "Зачет",
                Attendance = true,
                Student = studentInvalidGrades,
                Lesson = _stm32Lesson1Past
            };
            var invalidGrade2 = new AcademicPerformance
            {
                PerformanceID = 6002,
                StudentID = studentInvalidGrades.UserID,
                LessonID = _scratchLesson1Past.LessonID,
                Grade = "Н/А",
                Attendance = true,
                Student = studentInvalidGrades,
                Lesson = _scratchLesson1Past
            };
            var entitiesScenario3 = new List<object>
            {
                _teacherRole, _studentRole, _group9A, _group9B,
                _teacherUser, studentInvalidGrades,
                _mathSubject, _physicsSubject, _stm32Subject, _scratchSubject,
                _stm32StudyGroup, _scratchStudyGroup,
                _classroom101, _classroom102,
                _stm32Lesson1Past, _stm32Lesson2Future, _scratchLesson1Past, _scratchLesson2Future,
                invalidGrade1, invalidGrade2
            };
            var vmInvalidGrades = await CreateViewModel(studentInvalidGrades, entitiesScenario3.ToArray());
            Assert.Equal(0.0, vmInvalidGrades.AverageGradeValue);
            Assert.Equal("0.00", vmInvalidGrades.AverageGradeDisplayText);
        }
    }
}