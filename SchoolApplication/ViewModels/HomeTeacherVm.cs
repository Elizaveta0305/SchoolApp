using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class HomeTeacherVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        [ObservableProperty]
        private string _currentTeacherFullName = "Неизвестный";

        [ObservableProperty]
        private ObservableCollection<LessonDisplayModel> _upcomingLessons = new ObservableCollection<LessonDisplayModel>();

        [ObservableProperty]
        private int _currentStudentCount;

        [ObservableProperty]
        private double _averageGradeValue;
        public string AverageGradeDisplayText => AverageGradeValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        [ObservableProperty]
        private int _conductedLessonsCount;

        [ObservableProperty]
        private int _totalLessonsInAcademicYear;

        public string ConductedLessonsDisplayText => TotalLessonsInAcademicYear > 0 ? $"{ConductedLessonsCount} из {TotalLessonsInAcademicYear} ({((double)ConductedLessonsCount * 100 / TotalLessonsInAcademicYear).ToString("F0")}%)" : "0 занятий";


        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        private User? _currentTeacher;

        public HomeTeacherVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentTeacher = message.Value;
                await LoadAllTeacherHomeData();
            }
            else
            {
                _currentTeacher = null;
                CurrentTeacherFullName = "Неизвестный";
                UpcomingLessons.Clear();
                CurrentStudentCount = 0;
                AverageGradeValue = 0;
                ConductedLessonsCount = 0;
                TotalLessonsInAcademicYear = 0;
            }
        }

        private async Task LoadAllTeacherHomeData()
        {
            if (_currentTeacher == null)
            {
                CurrentTeacherFullName = "Неизвестный";
                return;
            }

            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var teacher = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserID == _currentTeacher.UserID);

                    if (teacher != null)
                    {
                        CurrentTeacherFullName = $"{teacher.FirstName} {teacher.MiddleName}";
                        var now = DateTime.Now;
                        var today = DateOnly.FromDateTime(now);
                        var currentTime = now.TimeOfDay;

                        var lessons = await dbContext.Lessons
                            .Include(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Subject)
                            .Include(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Group)
                            .Include(l => l.StudyGroup)
                                .ThenInclude(sg => sg.Teacher)
                            .Include(l => l.Classroom)
                            .AsNoTracking()
                            .Where(l => l.StudyGroup != null && l.StudyGroup.TeacherID == _currentTeacher.UserID)
                            .Where(l => DateOnly.FromDateTime(l.LessonDate) >= today)
                            .OrderBy(l => l.LessonDate)
                            .ThenBy(l => l.LessonTime)
                            .Take(5)
                            .ToListAsync();

                        UpcomingLessons.Clear();

                        foreach (var lesson in lessons)
                        {
                            if (DateOnly.FromDateTime(lesson.LessonDate) == today && lesson.LessonTime < currentTime)
                            {
                                continue;
                            }

                            UpcomingLessons.Add(new LessonDisplayModel
                            {
                                LessonId = lesson.LessonID,
                                SubjectName = lesson.StudyGroup?.Subject?.SubjectName ?? "N/A",
                                TeacherFullName = $"{lesson.StudyGroup?.Teacher?.FirstName} {lesson.StudyGroup?.Teacher?.MiddleName}" ?? "N/A",
                                RoomNumber = lesson.Classroom?.RoomNumber ?? "N/A",
                                LessonDate = DateOnly.FromDateTime(lesson.LessonDate),
                                LessonTime = lesson.LessonTime,
                                GroupName = lesson.StudyGroup?.Group?.GroupName ?? "N/A",
                                FullLessonDateTime = lesson.LessonDate.Add(lesson.LessonTime)
                            });
                        }

                        var teacherGroupIds = await dbContext.StudyGroups
                            .Where(sg => sg.TeacherID == _currentTeacher.UserID)
                            .Select(sg => sg.GroupID)
                            .Distinct()
                            .ToListAsync();

                        CurrentStudentCount = await dbContext.Users
                            .Where(u => u.RoleID == 3 && u.GroupID.HasValue && teacherGroupIds.Contains(u.GroupID.Value))
                            .CountAsync();

                        var stringGrades = await dbContext.AcademicPerformance
                            .Where(ap => ap.Grade != null && ap.Grade != "")
                            .Select(ap => ap.Grade)
                            .ToListAsync();

                        var numericGrades = new List<double>();

                        foreach (var gradeStr in stringGrades)
                        {
                            double gradeValue;
                            bool parsed = false;

                            if (double.TryParse(gradeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gradeValue))
                            {
                                numericGrades.Add(gradeValue);
                                parsed = true;
                            }
                            else
                            {
                                if (double.TryParse(gradeStr, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("ru-RU"), out gradeValue))
                                {
                                    numericGrades.Add(gradeValue);
                                    parsed = true;
                                }
                            }
                        }

                        if (numericGrades.Any())
                        {
                            AverageGradeValue = numericGrades.Average();
                            OnPropertyChanged(nameof(AverageGradeDisplayText));
                        }
                        else
                        {
                            AverageGradeValue = 0;
                            OnPropertyChanged(nameof(AverageGradeDisplayText));
                        }

                        DateTime academicYearStart;
                        DateTime academicYearEnd;

                        if (now.Month >= 9)
                        {
                            academicYearStart = new DateTime(now.Year, 9, 1);
                            academicYearEnd = new DateTime(now.Year + 1, 8, 31).AddDays(1).AddTicks(-1);
                        }
                        else
                        {
                            academicYearStart = new DateTime(now.Year - 1, 9, 1);
                            academicYearEnd = new DateTime(now.Year, 8, 31).AddDays(1).AddTicks(-1);
                        }

                        var allLessonsInYear = await dbContext.Lessons
                            .Where(l => l.StudyGroup != null && l.StudyGroup.TeacherID == _currentTeacher.UserID)
                            .Where(l => l.LessonDate >= academicYearStart && l.LessonDate <= academicYearEnd)
                            .ToListAsync();

                        TotalLessonsInAcademicYear = allLessonsInYear.Count;

                        ConductedLessonsCount = allLessonsInYear
                            .Where(l => l.LessonDate.Add(l.LessonTime) < now)
                            .Count();

                        OnPropertyChanged(nameof(ConductedLessonsDisplayText));

                        if (TotalLessonsInAcademicYear == 0)
                        {
                            TotalLessonsInAcademicYear = ConductedLessonsCount > 0 ? ConductedLessonsCount : 1;
                            OnPropertyChanged(nameof(TotalLessonsInAcademicYear));
                            OnPropertyChanged(nameof(ConductedLessonsDisplayText));
                        }

                    }
                    else
                    {
                        CurrentTeacherFullName = "Неизвестный";
                        UpcomingLessons.Clear();
                        CurrentStudentCount = 0;
                        AverageGradeValue = 0;
                        ConductedLessonsCount = 0;
                        TotalLessonsInAcademicYear = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                CurrentTeacherFullName = "Неизвестный";
                UpcomingLessons.Clear();
                CurrentStudentCount = 0;
                AverageGradeValue = 0;
                ConductedLessonsCount = 0;
                TotalLessonsInAcademicYear = 0;
            }
        }
    }
}