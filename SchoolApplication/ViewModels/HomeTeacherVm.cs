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
                    }
                    else
                    {
                        CurrentTeacherFullName = "Неизвестный";
                    }
                }
            }
            catch (Exception)

            {
                CurrentTeacherFullName = "Неизвестный";
            }
        }
    }
}