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
using System.Linq;
using System.Threading.Tasks;

namespace SchoolApplication.ViewModels
{
    public partial class HomeVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        [ObservableProperty]
        private ObservableCollection<LessonDisplayModel> _upcomingLessons = new ObservableCollection<LessonDisplayModel>();

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private User? _currentUser;

        public HomeVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
            WelcomeMessage = "Добро пожаловать!";
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentUser = message.Value;
                WelcomeMessage = $"Рады вас видеть, {_currentUser.FirstName}!";
                await LoadUpcomingLessons();
            }
            else
            {
                _currentUser = null;
                WelcomeMessage = "Добро пожаловать!";
                UpcomingLessons.Clear();
            }
        }

        [RelayCommand]
        public async Task LoadUpcomingLessons()
        {

            if (_currentUser == null)
            {
                UpcomingLessons.Clear();
                return;
            }

            if (_currentUser.GroupID == null)
            {
                UpcomingLessons.Clear();
                return;
            }

            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var now = DateTime.Now;

                    var allGroupLessons = await dbContext.Lessons
                        .Include(l => l.StudyGroup)
                            .ThenInclude(sg => sg.Subject)
                        .Include(l => l.StudyGroup)
                            .ThenInclude(sg => sg.Teacher)
                        .Include(l => l.Classroom)
                        .Where(l => l.StudyGroup != null && l.StudyGroup.GroupID == _currentUser.GroupID)
                        .ToListAsync();


                    var upcoming = allGroupLessons
                        .Where(l => l.LessonDate.Add(l.LessonTime) > now)
                        .OrderBy(l => l.LessonDate)
                        .ThenBy(l => l.LessonTime)
                        .Take(4)
                        .ToList();


                    UpcomingLessons.Clear();
                    if (upcoming.Any())
                    {
                        foreach (var lesson in upcoming)
                        {
                            UpcomingLessons.Add(new LessonDisplayModel
                            {
                                LessonId = lesson.LessonID,
                                SubjectName = lesson.StudyGroup?.Subject?.SubjectName ?? "Неизвестный предмет",
                                TeacherFullName = lesson.StudyGroup?.Teacher != null
                                    ? $"{lesson.StudyGroup.Teacher.LastName} {lesson.StudyGroup.Teacher.FirstName[0]}.{(string.IsNullOrEmpty(lesson.StudyGroup.Teacher.MiddleName) ? "" : lesson.StudyGroup.Teacher.MiddleName[0] + ".")}"
                                    : "Неизвестный преподаватель",
                                RoomNumber = lesson.Classroom?.RoomNumber ?? "Н/Д",
                                LessonDate = DateOnly.FromDateTime(lesson.LessonDate),
                                LessonTime = lesson.LessonTime
                            });
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Нет предстоящих занятий для отображения.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке ближайших уроков: {ex.Message}");
            }
        }
    }
}