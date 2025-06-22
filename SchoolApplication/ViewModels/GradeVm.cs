using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.Models.DisplayModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

// Эта директива ОЧЕНЬ ВАЖНА для использования ValueMessage<T> и Recipient<T>
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SchoolApplication.ViewModels
{
    public partial class GradeVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private string _studentFullName = "Неизвестно";

        [ObservableProperty]
        private string _studentGroupName = "Неизвестно";

        [ObservableProperty]
        private string _studentSubjects = "Предметы не определены";

        [ObservableProperty]
        private ObservableCollection<GradeDisplayModel> _studentGrades = new();

        private User? _currentUser;

        public GradeVm(IDbContextFactory<ApplicationDbContext> dbContextFactory, IMessenger messenger)
        {
            _dbContextFactory = dbContextFactory;
            _messenger = messenger;
            // Правильная регистрация: ViewModel сама является получателем сообщения.
            // RegisterAll(this) регистрирует все интерфейсы IRecipient, реализованные в этом классе.
            _messenger.RegisterAll(this);
        }

        // Правильная реализация метода Receive из интерфейса IRecipient<UserAuthenticatedMessage>.
        // Поле User в сообщении UserAuthenticatedMessage теперь доступно через свойство Value.
        public async void Receive(UserAuthenticatedMessage message)
        {
            _currentUser = message.Value; // Доступ к данным сообщения через .Value
            if (_currentUser != null && _currentUser.Role?.RoleName == "Ученик")
            {
                await LoadStudentDataAndGrades(_currentUser);
            }
            else
            {
                ResetViewModelProperties();
            }
        }

        private async Task LoadStudentDataAndGrades(User student)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var studentData = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.Group)
                    .ThenInclude(g => g.StudyGroups) // <--- Убран '!'
                        .ThenInclude(sg => sg.Subject)
// ...
                        .Include(u => u.AcademicPerformanceAsStudent) // <--- Убран '!'
                            .ThenInclude(ap => ap.Lesson)
                                .ThenInclude(l => l.StudyGroup)
                                    .ThenInclude(sg => sg.Subject)
// ...
                                    .Include(u => u.AcademicPerformanceAsStudent) // <--- Убран '!'
                                        .ThenInclude(ap => ap.Lesson)
                                            .ThenInclude(l => l.StudyGroup)
                                                .ThenInclude(sg => sg.Teacher)// Включаем учителя!
                    .FirstOrDefaultAsync(u => u.UserID == student.UserID);

                if (studentData != null)
                {
                    StudentFullName = $"{studentData.LastName ?? ""} {studentData.FirstName ?? ""} {studentData.MiddleName ?? ""}".Trim();
                    StudentGroupName = studentData.Group?.GroupName ?? "Группа не определена";
                    StudentSubjects = studentData.Group?.StudyGroups != null && studentData.Group.StudyGroups.Any()
                        ? string.Join(", ", studentData.Group.StudyGroups
                            .Where(sg => sg.Subject != null)
                            .Select(sg => sg.Subject?.SubjectName)
                            .Where(name => !string.IsNullOrEmpty(name)))
                        : "Предметы не определены";

                    var gradesList = new List<GradeDisplayModel>();
                    // *** AND HERE ***
                    // Use AcademicPerformanceAsStudent when iterating the collection
                    if (studentData.AcademicPerformanceAsStudent != null)
                    {
                        foreach (var performance in studentData.AcademicPerformanceAsStudent
                            .OrderByDescending(ap => ap.Lesson?.LessonDate)
                            .ThenByDescending(ap => ap.Lesson?.LessonTime))
                        {
                            var lesson = performance.Lesson;

                            var studyGroup = lesson?.StudyGroup;
                            var subject = studyGroup?.Subject;
                            var teacher = studyGroup?.Teacher;

                            string teacherFullName = "Неизвестный преподаватель";
                            if (teacher != null)
                            {
                                var firstNameInitial = !string.IsNullOrEmpty(teacher.FirstName) ? teacher.FirstName[0].ToString() + "." : "";
                                var middleNameInitial = !string.IsNullOrEmpty(teacher.MiddleName) ? teacher.MiddleName[0].ToString() + "." : "";
                                teacherFullName = $"{teacher.LastName ?? ""} {firstNameInitial}{middleNameInitial}".Trim();
                            }

                            gradesList.Add(new GradeDisplayModel
                            {
                                PerformanceID = performance.PerformanceID,
                                SubjectName = subject?.SubjectName ?? "Неизвестный предмет",
                                TeacherFullName = teacherFullName,
                                LessonDate = DateOnly.FromDateTime(lesson?.LessonDate ?? DateTime.MinValue),
                                LessonTime = lesson?.LessonTime ?? TimeSpan.Zero,
                                GradeValue = performance.Grade ?? "Н/Д",
                                AttendanceMark = performance.Attendance,
                                Comment = performance.Comment ?? "Нет комментария"
                            });
                        }
                    }
                    StudentGrades = new ObservableCollection<GradeDisplayModel>(gradesList);
                }
                else
                {
                    ResetViewModelProperties();
                }
            }
        }

        private void ResetViewModelProperties()
        {
            StudentFullName = "Неизвестно";
            StudentGroupName = "Неизвестно";
            StudentSubjects = "Предметы не определены";
            StudentGrades.Clear();
        }
    }
}