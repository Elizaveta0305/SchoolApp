using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using System.Collections.ObjectModel;

namespace SchoolApplication.ViewModels
{
    public partial class HomeTeacherVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        [ObservableProperty]
        private string _currentTeacherFullName = "Неизвестный";

        [ObservableProperty]
        private ObservableCollection<LessonDisplayModel> _upcomingLessons = new ObservableCollection<LessonDisplayModel>();

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
                    }
                    else
                    {
                        CurrentTeacherFullName = "Неизвестный";
                        UpcomingLessons.Clear();
                    }
                }
            }
            catch (Exception ex)

            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                CurrentTeacherFullName = "Неизвестный";
                UpcomingLessons.Clear();
            }
        }
    }
}