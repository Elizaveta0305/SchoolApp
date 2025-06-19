using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

namespace SchoolApplication.ViewModels
{
    public partial class GradeVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private User? _currentUser;

        [ObservableProperty]
        private string _studentFullName = "Неизвестно";
        [ObservableProperty]
        private string _studentGroupName = "Неизвестно";
        [ObservableProperty]
        private string _studentSubjects = "Загрузка...";

        [ObservableProperty]
        private ObservableCollection<GradeDisplayModel> _studentGrades = new ObservableCollection<GradeDisplayModel>();

        public GradeVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentUser = message.Value;
                await LoadStudentDataAndGrades();
            }
            else
            {
                _currentUser = null;
                ClearStudentData();
            }
        }

        public async Task LoadStudentDataAndGrades()
        {
            if (_currentUser == null)
            {
                ClearStudentData();
                return;
            }

            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var student = await dbContext.Users
                        .Include(u => u.Group)
                            .ThenInclude(g => g.StudyGroups!)
                                .ThenInclude(sg => sg.Subject)
                        .FirstOrDefaultAsync(u => u.UserID == _currentUser.UserID);

                    if (student != null)
                    {
                        StudentFullName = $"{student.LastName} {student.FirstName}{(string.IsNullOrEmpty(student.MiddleName) ? "" : $" {student.MiddleName}")}";
                        StudentGroupName = student.Group?.GroupName ?? "Группа не определена";

                        var subjects = student.Group?.StudyGroups?
                            .Select(sg => sg.Subject?.SubjectName)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Distinct()
                            .ToList();

                        StudentSubjects = subjects != null && subjects.Any()
                            ? string.Join(", ", subjects)
                            : "Предметы не определены";

                    }
                    else
                    {
                        ClearStudentData();
                    }

                    var grades = await dbContext.AcademicPerformance
                        .Include(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Subject)
                        .Include(ap => ap.Lesson)
                            .ThenInclude(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Teacher)
                        .Where(ap => ap.StudentID == _currentUser.UserID)
                        .OrderByDescending(ap => ap.Lesson!.LessonDate)
                        .ThenByDescending(ap => ap.Lesson!.LessonTime)
                        .ToListAsync();

                    StudentGrades.Clear();
                    if (grades.Any())
                    {
                        foreach (var grade in grades)
                        {
                            StudentGrades.Add(new GradeDisplayModel
                            {
                                PerformanceID = grade.PerformanceID,
                                SubjectName = grade.Lesson?.StudyGroup?.Subject?.SubjectName ?? "Неизвестно",
                                TeacherFullName = grade.Lesson?.StudyGroup?.Teacher != null
                                    ? $"{grade.Lesson.StudyGroup.Teacher.LastName} {grade.Lesson.StudyGroup.Teacher.FirstName[0]}.{(string.IsNullOrEmpty(grade.Lesson.StudyGroup.Teacher.MiddleName) ? "" : grade.Lesson.StudyGroup.Teacher.MiddleName[0] + ".")}"
                                    : "Неизвестно",
                                LessonDate = DateOnly.FromDateTime(grade.Lesson?.LessonDate ?? DateTime.MinValue),
                                LessonTime = grade.Lesson?.LessonTime ?? TimeSpan.Zero,
                                GradeValue = grade.Grade ?? "-",
                                AttendanceMark = grade.Attendance,
                                Comment = grade.Comment ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClearStudentData();
                StudentGrades.Clear();
            }
        }

        private void ClearStudentData()
        {
            StudentFullName = "Неизвестно";
            StudentGroupName = "Неизвестно";
            StudentSubjects = "Предметы не определены";
            StudentGrades.Clear();
        }
    }
}