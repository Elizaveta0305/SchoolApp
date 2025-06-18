using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolApplication.ViewModels
{
    public partial class HomeVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private string _welcomeMessage = "Добро пожаловать!";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        [ObservableProperty]
        private ObservableCollection<LessonDisplayModel> _upcomingLessons = new ObservableCollection<LessonDisplayModel>();

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private User? _currentUser;

        [ObservableProperty]
        private int _absencesCount;

        public const double MaxAbsencesValue = 30.0;

        [ObservableProperty]
        private string _absencesDisplayText = "0 / 30";

        [ObservableProperty]
        private int _subjectsCount;

        [ObservableProperty]
        private double _averageGradeValue;

        [ObservableProperty]
        private string _averageGradeDisplayText = "0.00";

        [ObservableProperty]
        private string _academicYear = "Неизвестно";

        public HomeVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentUser = message.Value;
                await LoadAllHomeData();
            }
            else
            {
                _currentUser = null;
                WelcomeMessage = "Добро пожаловать!";
                UpcomingLessons.Clear();
                AbsencesCount = 0;
                AbsencesDisplayText = "0 / 30";
                SubjectsCount = 0;
                AverageGradeValue = 0.0;
                AverageGradeDisplayText = "0.00";
                AcademicYear = "Неизвестно";
            }
        }

        private async Task LoadAllHomeData()
        {
            if (_currentUser == null)
            {
                WelcomeMessage = "Добро пожаловать!";
                UpcomingLessons.Clear();
                AbsencesCount = 0;
                AbsencesDisplayText = "0 / 30";
                SubjectsCount = 0;
                AverageGradeValue = 0.0;
                AverageGradeDisplayText = "0.00";
                AcademicYear = "Неизвестно";
                return;
            }

            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var student = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserID == _currentUser.UserID);

                    if (student != null)
                    {
                        WelcomeMessage = $"Рады вас видеть, {student.FirstName}!";
                        if (_currentUser.GroupID == null && student.GroupID != null)
                        {
                            _currentUser.GroupID = student.GroupID;
                        }
                    }
                    else
                    {
                        WelcomeMessage = "Добро пожаловать!";
                    }

                    await LoadUpcomingLessonsInternal(dbContext);
                    await LoadAnalyticsData(dbContext);
                }
            }
            catch (Exception)
            {
                WelcomeMessage = "Добро пожаловать!";
                UpcomingLessons.Clear();
                AbsencesCount = 0;
                AbsencesDisplayText = "0 / 30";
                SubjectsCount = 0;
                AverageGradeValue = 0.0;
                AverageGradeDisplayText = "0.00";
                AcademicYear = "Ошибка загрузки";

            }
        }

        private async Task LoadUpcomingLessonsInternal(ApplicationDbContext dbContext)
        {
            UpcomingLessons.Clear();

            if (_currentUser?.GroupID == null)
            {
                return;
            }

            try
            {
                var now = DateTime.Now;

                var query = dbContext.Lessons
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Subject)
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Teacher)
                    .Include(l => l.Classroom)
                    .Where(l => l.StudyGroup!.GroupID == _currentUser.GroupID);

                var lessonsFromDb = await query.ToListAsync();

                var upcoming = lessonsFromDb
                    .Select(l => new LessonDisplayModel
                    {
                        LessonId = l.LessonID,
                        SubjectName = l.StudyGroup?.Subject?.SubjectName ?? "Неизвестный предмет",
                        TeacherFullName = l.StudyGroup?.Teacher != null
                            ? $"{l.StudyGroup.Teacher.LastName} {l.StudyGroup.Teacher.FirstName[0]}.{(string.IsNullOrEmpty(l.StudyGroup.Teacher.MiddleName) ? "" : l.StudyGroup.Teacher.MiddleName[0] + ".")}"
                            : "Неизвестный преподаватель",
                        RoomNumber = l.Classroom?.RoomNumber ?? "Н/Д",
                        LessonDate = DateOnly.FromDateTime(l.LessonDate),
                        LessonTime = l.LessonTime,
                        FullLessonDateTime = l.LessonDate.Add(l.LessonTime)
                    })
                    .Where(ldm => ldm.FullLessonDateTime > now)
                    .OrderBy(ldm => ldm.FullLessonDateTime)
                    .Take(4)
                    .ToList();

                if (upcoming.Any())
                {
                    foreach (var lesson in upcoming)
                    {
                        UpcomingLessons.Add(lesson);
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private async Task LoadAnalyticsData(ApplicationDbContext dbContext)
        {
            if (_currentUser == null || _currentUser.GroupID == null)
            {
                AbsencesCount = 0;
                AbsencesDisplayText = "0 / 30";
                SubjectsCount = 0;
                AverageGradeValue = 0.0;
                AverageGradeDisplayText = "0.00";
                AcademicYear = "Неизвестно";
                return;
            }

            try
            {
                var absences = await dbContext.AcademicPerformance
                    .Where(ap => ap.StudentID == _currentUser.UserID && ap.Attendance == false)
                    .CountAsync();
                AbsencesCount = (int)Math.Min(absences, MaxAbsencesValue);
                AbsencesDisplayText = $"{absences} / {MaxAbsencesValue}";

                var subjectsCount = await dbContext.StudyGroups
                    .Where(sg => sg.GroupID == _currentUser.GroupID)
                    .Select(sg => sg.SubjectID)
                    .Distinct()
                    .CountAsync();
                SubjectsCount = subjectsCount;

                var studentGradesQuery = dbContext.AcademicPerformance
                    .Where(ap => ap.StudentID == _currentUser.UserID &&
                                 !string.IsNullOrEmpty(ap.Grade));

                var gradeStrings = await studentGradesQuery
                    .Select(ap => ap.Grade)
                    .ToListAsync();

                var validGrades = new List<double>();

                foreach (var gradeStr in gradeStrings)
                {
                    if (double.TryParse(gradeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double gradeValue))
                    {
                        validGrades.Add(gradeValue);
                    }
                }

                if (validGrades.Any())
                {
                    double averageGrade = validGrades.Average();
                    AverageGradeValue = averageGrade;
                    AverageGradeDisplayText = averageGrade.ToString("F2");
                }
                else
                {
                    AverageGradeValue = 0.0;
                    AverageGradeDisplayText = "Н/Д";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                AbsencesCount = 0;
                AbsencesDisplayText = "0 / 30";
                SubjectsCount = 0;
                AverageGradeValue = 0.0;
                AverageGradeDisplayText = "0.00";
                AcademicYear = "Ошибка загрузки";
            }
        }
    }
}