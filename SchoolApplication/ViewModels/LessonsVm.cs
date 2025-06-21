using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.Messages;
using System.Globalization;

namespace SchoolApplication.ViewModels
{
    public partial class LessonsVm : ObservableObject
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<LessonDisplayModel> _allStudentLessons = new ObservableCollection<LessonDisplayModel>();

        private User? _currentUser;

        internal TaskCompletionSource<bool> _initialLoadCompletionSource = new TaskCompletionSource<bool>();

        public LessonsVm(IDbContextFactory<ApplicationDbContext> dbContextFactory, IMessenger? messenger = null)
        {
            _dbContextFactory = dbContextFactory;
            _messenger = messenger ?? WeakReferenceMessenger.Default;

            _messenger.Register<LessonsVm, UserAuthenticatedMessage>(this, async (r, m) =>
            {
                r._currentUser = m.Value;
                if (r.LoadAllStudentLessonsCommand.CanExecute(null))
                {
                    await r.LoadAllStudentLessonsCommand.ExecuteAsync(null);
                }
                r._initialLoadCompletionSource.TrySetResult(true);
            });
        }

        [RelayCommand]
        public async Task LoadAllStudentLessons()
        {
            if (_currentUser == null || _currentUser.GroupID == null)
            {
                AllStudentLessons.Clear();
                return;
            }

            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var lessons = await dbContext.Lessons
                        .Include(l => l.StudyGroup)
                            .ThenInclude(sg => sg.Subject)
                        .Include(l => l.StudyGroup)
                            .ThenInclude(sg => sg.Teacher)
                        .Include(l => l.Classroom)
                        .Where(l => l.StudyGroup != null && l.StudyGroup.GroupID == _currentUser.GroupID)
                        .OrderBy(l => l.LessonDate)
                        .ThenBy(l => l.LessonTime)
                        .ToListAsync();

                    AllStudentLessons.Clear();
                    foreach (var lesson in lessons)
                    {
                        AllStudentLessons.Add(new LessonDisplayModel
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке всех занятий: {ex.Message}");
                AllStudentLessons.Clear();
            }
        }
    }
}