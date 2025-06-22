using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models.DisplayModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AppModels = SchoolApplication.Models;

namespace SchoolApplication.ViewModels
{
    public partial class LessonTeacherVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private AppModels.User? _currentTeacherUser;

        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<LessonTeacherDisplayModel> _lessonsCollection = new();

        [ObservableProperty]
        private ObservableCollection<AppModels.Group> _groups = new();
        [ObservableProperty]
        private AppModels.Group? _selectedGroup;

        [ObservableProperty]
        private ObservableCollection<AppModels.Subject> _subjects = new();
        [ObservableProperty]
        private AppModels.Subject? _selectedSubject;

        [ObservableProperty]
        private ObservableCollection<AppModels.Classroom> _classrooms = new();
        [ObservableProperty]
        private AppModels.Classroom? _selectedClassroom;

        [ObservableProperty]
        private ObservableCollection<string> _actionTypes = new() { "Добавить", "Обновить", "Удалить" };
        [ObservableProperty]
        private string? _selectedActionType;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private int _selectedHour = 9;
        [ObservableProperty]
        private int _selectedMinute = 0;

        [ObservableProperty]
        private string? _lessonTopicInput;

        [ObservableProperty]
        private LessonTeacherDisplayModel? _selectedLessonToEdit;

        public LessonTeacherVm(IDbContextFactory<ApplicationDbContext> dbContextFactory, IMessenger messenger)
        {
            _dbContextFactory = dbContextFactory;
            _messenger = messenger;
            _messenger.Register<UserAuthenticatedMessage>(this);
            _ = LoadInitialDataAsync();
            
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentTeacherUser = message.Value;
                await LoadInitialDataAsync();
            }
            else
            {
                _currentTeacherUser = null;
                LessonsCollection.Clear();
                Groups.Clear();
                Subjects.Clear();
                Classrooms.Clear();
                ClearActionInputFields();
            }
        }

        private async Task LoadInitialDataAsync()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (_currentTeacherUser != null)
                {
                    var teacherStudyGroups = await dbContext.StudyGroups
                        .Where(sg => sg.TeacherID == _currentTeacherUser.UserID)
                        .Include(sg => sg.Group)
                        .Include(sg => sg.Subject)
                        .ToListAsync();

                    Groups.Clear();
                    foreach (var sg in teacherStudyGroups.Select(sg => sg.Group!).DistinctBy(g => g.GroupID).OrderBy(g => g.GroupName))
                    {
                        Groups.Add(sg);
                    }

                    Subjects.Clear();
                    foreach (var sg in teacherStudyGroups.Select(sg => sg.Subject!).DistinctBy(s => s.SubjectID).OrderBy(s => s.SubjectName))
                    {
                        Subjects.Add(sg);
                    }

                    Classrooms.Clear();
                    var allClassrooms = await dbContext.Classrooms.OrderBy(c => c.RoomNumber).ToListAsync();
                    foreach (var classroom in allClassrooms)
                    {
                        Classrooms.Add(classroom);
                    }

                    await LoadLessonsDataAsync();
                }
                else
                {
                    Groups.Clear();
                    Subjects.Clear();
                    Classrooms.Clear();
                    LessonsCollection.Clear();
                    ClearActionInputFields();
                }
            }
        }

        [RelayCommand]
        private async Task LoadLessonsDataAsync()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (_currentTeacherUser == null)
                {
                    LessonsCollection.Clear();
                    return;
                }

                var query = dbContext.Lessons
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Group)
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Subject)
                    .Include(l => l.Classroom)
                    .AsNoTracking()
                    .Where(l => l.StudyGroup!.TeacherID == _currentTeacherUser.UserID);

                if (SelectedGroup != null)
                {
                    query = query.Where(l => l.StudyGroup!.GroupID == SelectedGroup.GroupID);
                }
                if (SelectedSubject != null)
                {
                    query = query.Where(l => l.StudyGroup!.SubjectID == SelectedSubject.SubjectID);
                }

                var lessons = await query.OrderByDescending(l => l.LessonDate).ThenByDescending(l => l.LessonTime).ToListAsync();

                LessonsCollection.Clear();
                foreach (var lesson in lessons)
                {
                    LessonsCollection.Add(new LessonTeacherDisplayModel
                    {
                        LessonId = lesson.LessonID,
                        GroupName = lesson.StudyGroup?.Group?.GroupName ?? "N/A",
                        SubjectName = lesson.StudyGroup?.Subject?.SubjectName ?? "N/A",
                        LessonDate = lesson.LessonDate,
                        LessonTime = lesson.LessonTime,
                        Topic = lesson.Topic ?? "Без темы",
                        ClassroomNumber = lesson.Classroom?.RoomNumber
                    });
                }
            }
        }

        partial void OnSelectedGroupChanged(AppModels.Group? value)
        {
            _ = LoadLessonsDataAsync();
        }

        partial void OnSelectedSubjectChanged(AppModels.Subject? value)
        {
            _ = LoadLessonsDataAsync();
        }

        [RelayCommand]
        private async Task PerformLessonActionAsync()
        {
            if (_currentTeacherUser == null)
            {
                return;
            }

            if (SelectedGroup == null || SelectedSubject == null || SelectedClassroom == null || string.IsNullOrWhiteSpace(LessonTopicInput))
            {
                return;
            }

            if (SelectedHour < 0 || SelectedHour > 23 || SelectedMinute < 0 || SelectedMinute > 59)
            {
                return;
            }

            if ((SelectedActionType == "Обновить" || SelectedActionType == "Удалить") && SelectedLessonToEdit == null)
            {
                return;
            }
            if (SelectedActionType == null)
            {
                return;
            }


            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var studyGroup = await dbContext.StudyGroups
                    .FirstOrDefaultAsync(sg => sg.GroupID == SelectedGroup.GroupID &&
                                               sg.SubjectID == SelectedSubject.SubjectID &&
                                               sg.TeacherID == _currentTeacherUser.UserID);

                if (studyGroup == null)
                {
                    return;
                }

                var lessonTime = new TimeSpan(SelectedHour, SelectedMinute, 0);

                AppModels.Lesson? lessonToModify = null;

                if (SelectedLessonToEdit != null)
                {
                    lessonToModify = await dbContext.Lessons.FindAsync(SelectedLessonToEdit.LessonId);
                    if (lessonToModify == null)
                    {
                        return;
                    }
                }

                switch (SelectedActionType)
                {
                    case "Добавить":
                        var existingLesson = await dbContext.Lessons
                            .FirstOrDefaultAsync(l => l.StudyGroupID == studyGroup.StudyGroupID &&
                                                      l.LessonDate == SelectedDate.Date &&
                                                      l.LessonTime == lessonTime &&
                                                      l.Topic == LessonTopicInput &&
                                                      l.ClassroomID == SelectedClassroom.ClassroomID);

                        if (existingLesson != null)
                        {
                            return;
                        }

                        var newLesson = new AppModels.Lesson
                        {
                            StudyGroupID = studyGroup.StudyGroupID,
                            LessonDate = SelectedDate.Date,
                            LessonTime = lessonTime,
                            Topic = LessonTopicInput!,
                            ClassroomID = SelectedClassroom.ClassroomID
                        };
                        dbContext.Lessons.Add(newLesson);
                        break;

                    case "Обновить":
                        if (lessonToModify == null)
                        {
                            return;
                        }

                        var duplicateCheck = await dbContext.Lessons
                            .FirstOrDefaultAsync(l => l.LessonID != lessonToModify.LessonID &&
                                                      l.StudyGroupID == studyGroup.StudyGroupID &&
                                                      l.LessonDate == SelectedDate.Date &&
                                                      l.LessonTime == lessonTime &&
                                                      l.Topic == LessonTopicInput &&
                                                      l.ClassroomID == SelectedClassroom.ClassroomID);
                        if (duplicateCheck != null)
                        {
                            return;
                        }

                        lessonToModify.LessonDate = SelectedDate.Date;
                        lessonToModify.LessonTime = lessonTime;
                        lessonToModify.Topic = LessonTopicInput!;
                        lessonToModify.ClassroomID = SelectedClassroom.ClassroomID;
                        dbContext.Lessons.Update(lessonToModify);
                        break;

                    case "Удалить":
                        if (lessonToModify == null)
                        {
                            return;
                        }

                        var hasPerformances = await dbContext.AcademicPerformance.AnyAsync(ap => ap.LessonID == lessonToModify.LessonID);
                        if (hasPerformances)
                        {
                            return;
                        }
                        dbContext.Lessons.Remove(lessonToModify);
                        break;
                    default:
                        return;
                }

                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();

                await LoadLessonsDataAsync();
                _messenger.Send(new LessonsUpdatedMessage(true));
                ClearActionInputFields();
            }
        }

        [RelayCommand]
        private async Task EditLesson(LessonTeacherDisplayModel? lesson)
        {
            if (lesson == null) return;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var fullLesson = await dbContext.Lessons
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Group)
                    .Include(l => l.StudyGroup)
                        .ThenInclude(sg => sg!.Subject)
                    .Include(l => l.Classroom)
                    .FirstOrDefaultAsync(l => l.LessonID == lesson.LessonId);

                if (fullLesson == null || fullLesson.StudyGroup == null || fullLesson.StudyGroup.Group == null || fullLesson.StudyGroup.Subject == null)
                {
                    return;
                }

                SelectedLessonToEdit = lesson;

                SelectedGroup = Groups.FirstOrDefault(g => g.GroupID == fullLesson.StudyGroup.GroupID);
                SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectID == fullLesson.StudyGroup.SubjectID);
                SelectedClassroom = Classrooms.FirstOrDefault(c => c.ClassroomID == fullLesson.ClassroomID);

                SelectedDate = fullLesson.LessonDate;
                SelectedHour = fullLesson.LessonTime.Hours;
                SelectedMinute = fullLesson.LessonTime.Minutes;
                LessonTopicInput = fullLesson.Topic;
                SelectedActionType = "Обновить";
            }
        }

        [RelayCommand]
        private async Task DeleteLesson(LessonTeacherDisplayModel? lesson)
        {
            if (lesson == null) return;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var lessonToDelete = await dbContext.Lessons.FindAsync(lesson.LessonId);
                if (lessonToDelete != null)
                {
                    var hasPerformances = await dbContext.AcademicPerformance.AnyAsync(ap => ap.LessonID == lessonToDelete.LessonID);
                    if (hasPerformances)
                    {
                        return;
                    }

                    dbContext.Lessons.Remove(lessonToDelete);
                    await dbContext.SaveChangesAsync();
                    dbContext.ChangeTracker.Clear();
                    await LoadLessonsDataAsync();
                    _messenger.Send(new LessonsUpdatedMessage(true));
                }
                
                ClearActionInputFields();
            }
        }

        private void ClearActionInputFields()
        {
            SelectedGroup = null;
            SelectedSubject = null;
            SelectedClassroom = null;
            SelectedDate = DateTime.Today;
            SelectedHour = 9;
            SelectedMinute = 0;
            LessonTopicInput = null;
            SelectedActionType = null;
            SelectedLessonToEdit = null;
        }
    }
}