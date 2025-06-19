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

        public LessonTeacherVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
            _ = LoadInitialDataAsync();
        }

        public async void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentTeacherUser = message.Value;
                Debug.WriteLine($"[LessonTeacherVm] Пользователь аутентифицирован: {_currentTeacherUser.Username}");
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
                Debug.WriteLine("[LessonTeacherVm] Пользователь вышел из системы.");
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
                Debug.WriteLine($"[LessonTeacherVm] Загружено {LessonsCollection.Count} занятий.");
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
                Debug.WriteLine("[LessonTeacherVm] Ошибка: Пользователь не аутентифицирован. Невозможно выполнить действие.");
                return;
            }

            if (SelectedGroup == null || SelectedSubject == null || SelectedClassroom == null || string.IsNullOrWhiteSpace(LessonTopicInput))
            {
                Debug.WriteLine("[LessonTeacherVm] Ошибка: Выберите все необходимые поля (Группа, Предмет, Тема, Кабинет).");
                return;
            }

            if (SelectedHour < 0 || SelectedHour > 23 || SelectedMinute < 0 || SelectedMinute > 59)
            {
                Debug.WriteLine("[LessonTeacherVm] Ошибка: Некорректное значение для часов или минут.");
                return;
            }

            if ((SelectedActionType == "Обновить" || SelectedActionType == "Удалить") && SelectedLessonToEdit == null)
            {
                Debug.WriteLine("[LessonTeacherVm] Ошибка: Для обновления или удаления необходимо выбрать занятие из списка.");
                return;
            }
            if (SelectedActionType == null)
            {
                Debug.WriteLine("[LessonTeacherVm] Ошибка: Выберите тип действия (Добавить, Обновить, Удалить).");
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
                    Debug.WriteLine("[LessonTeacherVm] Ошибка: Не удалось найти связку Группа-Предмет для текущего преподавателя. Убедитесь, что вы ведете этот предмет в этой группе.");
                    return;
                }

                var lessonTime = new TimeSpan(SelectedHour, SelectedMinute, 0);

                AppModels.Lesson? lessonToModify = null;

                if (SelectedLessonToEdit != null)
                {
                    lessonToModify = await dbContext.Lessons.FindAsync(SelectedLessonToEdit.LessonId);
                    if (lessonToModify == null)
                    {
                        Debug.WriteLine("[LessonTeacherVm] Ошибка: Занятие для изменения не найдено в базе данных.");
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
                            Debug.WriteLine("[LessonTeacherVm] Ошибка: Занятие с такими параметрами уже существует. Используйте 'Обновить'.");
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
                        Debug.WriteLine($"[LessonTeacherVm] Добавление занятия: {newLesson.Topic} для группы {SelectedGroup.GroupName} по предмету {SelectedSubject.SubjectName}");
                        break;

                    case "Обновить":
                        if (lessonToModify == null)
                        {
                            Debug.WriteLine("[LessonTeacherVm] Ошибка: Занятие для обновления не выбрано.");
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
                            Debug.WriteLine("[LessonTeacherVm] Ошибка: Занятие с такими обновленными параметрами уже существует.");
                            return;
                        }

                        lessonToModify.LessonDate = SelectedDate.Date;
                        lessonToModify.LessonTime = lessonTime;
                        lessonToModify.Topic = LessonTopicInput!;
                        lessonToModify.ClassroomID = SelectedClassroom.ClassroomID;
                        dbContext.Lessons.Update(lessonToModify);
                        Debug.WriteLine($"[LessonTeacherVm] Обновление занятия ID {lessonToModify.LessonID}: Новая тема: {lessonToModify.Topic}");
                        break;

                    case "Удалить":
                        if (lessonToModify == null)
                        {
                            Debug.WriteLine("[LessonTeacherVm] Ошибка: Занятие для удаления не выбрано.");
                            return;
                        }

                        var hasPerformances = await dbContext.AcademicPerformance.AnyAsync(ap => ap.LessonID == lessonToModify.LessonID);
                        if (hasPerformances)
                        {
                            Debug.WriteLine("[LessonTeacherVm] Ошибка: Невозможно удалить занятие, так как с ним связаны оценки. Сначала удалите оценки.");
                            return;
                        }
                        dbContext.Lessons.Remove(lessonToModify);
                        Debug.WriteLine($"[LessonTeacherVm] Удаление занятия ID {lessonToModify.LessonID}");
                        break;
                    default:
                        Debug.WriteLine("[LessonTeacherVm] Ошибка: Неизвестный тип действия.");
                        return;
                }

                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();

                await LoadLessonsDataAsync();
                WeakReferenceMessenger.Default.Send(new LessonsUpdatedMessage(true));
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
                    Debug.WriteLine("[LessonTeacherVm] Не удалось загрузить полную информацию для редактирования занятия.");
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

                Debug.WriteLine($"[LessonTeacherVm] Загружено для редактирования: Тема: {lesson.Topic}, Группа: {lesson.GroupName}, Предмет: {lesson.SubjectName}, Кабинет: {lesson.ClassroomNumber}");
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
                        Debug.WriteLine("[LessonTeacherVm] Ошибка: Невозможно удалить занятие, так как с ним связаны оценки. Сначала удалите оценки.");
                        return;
                    }

                    dbContext.Lessons.Remove(lessonToDelete);
                    await dbContext.SaveChangesAsync();
                    dbContext.ChangeTracker.Clear();
                    await LoadLessonsDataAsync();
                    WeakReferenceMessenger.Default.Send(new LessonsUpdatedMessage(true));
                    Debug.WriteLine($"[LessonTeacherVm] Занятие ID {lesson.LessonId} успешно удалено.");
                }
                else
                {
                    Debug.WriteLine($"[LessonTeacherVm] Занятие с ID {lesson.LessonId} не найдено для удаления.");
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