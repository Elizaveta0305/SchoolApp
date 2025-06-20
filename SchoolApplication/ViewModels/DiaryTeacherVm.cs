using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolApplication.ViewModels
{
    public partial class DiaryTeacherVm : ObservableObject, IRecipient<UserAuthenticatedMessage>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private User? _currentTeacherUser;

        [ObservableProperty]
        private ObservableCollection<AcademicPerformanceDisplayModel> _diaryCollection = new();

        [ObservableProperty]
        private ObservableCollection<Group> _groups = new();
        [ObservableProperty]
        private Group? _selectedGroup;

        [ObservableProperty]
        private ObservableCollection<User> _studentsInSelectedGroup = new();
        [ObservableProperty]
        private User? _selectedStudent;

        [ObservableProperty]
        private ObservableCollection<Lesson> _lessonsForSelectedStudent = new();
        [ObservableProperty]
        private Lesson? _selectedLesson;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();
        [ObservableProperty]
        private Subject? _selectedSubject;

        [ObservableProperty]
        private ObservableCollection<string> _availableGrades = new() { "5", "4", "3", "2", "Н/А" };
        [ObservableProperty]
        private string? _selectedGrade;

        [ObservableProperty]
        private ObservableCollection<string> _actionTypes = new() { "Добавить", "Обновить", "Удалить" };
        [ObservableProperty]
        private string? _selectedActionType;

        [ObservableProperty]
        private string? _commentInput;

        public DiaryTeacherVm(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
        }

        public void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                _currentTeacherUser = message.Value;
                _ = LoadInitialDataAsync();
            }
            else
            {
                _currentTeacherUser = null;
                DiaryCollection.Clear();
                Groups.Clear();
                StudentsInSelectedGroup.Clear();
                LessonsForSelectedStudent.Clear();
                Subjects.Clear();
                ClearAllInputFields();
            }
        }

        internal async Task LoadInitialDataAsync()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (_currentTeacherUser != null)
                {
                    var teacherGroups = await dbContext.StudyGroups
                        .Where(sg => sg.TeacherID == _currentTeacherUser.UserID)
                        .Select(sg => sg.Group!)
                        .Distinct()
                        .OrderBy(g => g.GroupName)
                        .ToListAsync();

                    Groups.Clear();
                    foreach (var group in teacherGroups)
                    {
                        Groups.Add(group);
                    }

                    var teacherSubjects = await dbContext.StudyGroups
                        .Where(sg => sg.TeacherID == _currentTeacherUser.UserID)
                        .Select(sg => sg.Subject!)
                        .Distinct()
                        .OrderBy(s => s.SubjectName)
                        .ToListAsync();

                    Subjects.Clear();
                    foreach (var subject in teacherSubjects)
                    {
                        Subjects.Add(subject);
                    }

                    if (SelectedGroup == null && Groups.Any())
                    {
                        SelectedGroup = Groups.FirstOrDefault();
                    }
                    if (SelectedGroup != null)
                    {
                        await LoadStudentsAndLessonsForGroupAsync(SelectedGroup);
                        await LoadDiaryDataAsync();
                    }
                }
                else
                {
                    Groups.Clear();
                    Subjects.Clear();
                    StudentsInSelectedGroup.Clear();
                    LessonsForSelectedStudent.Clear();
                    DiaryCollection.Clear();
                }
            }
        }

        [RelayCommand]
        internal async Task LoadDiaryDataAsync()
        {
            DiaryCollection.Clear();

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (_currentTeacherUser == null)
                {
                    DiaryCollection.Clear();
                    return;
                }

                var query = dbContext.AcademicPerformance
                    .Include(ap => ap.Student)
                        .ThenInclude(s => s!.Group)
                    .Include(ap => ap.Lesson)
                        .ThenInclude(l => l!.StudyGroup)
                            .ThenInclude(sg => sg!.Subject)
                    .AsNoTracking()
                    .Where(ap => ap.Lesson!.StudyGroup!.TeacherID == _currentTeacherUser.UserID);

                if (SelectedGroup != null)
                {
                    query = query.Where(ap => ap.Student!.GroupID == SelectedGroup.GroupID);
                }
                if (SelectedStudent != null)
                {
                    query = query.Where(ap => ap.StudentID == SelectedStudent.UserID);
                }
                if (SelectedSubject != null)
                {
                    query = query.Where(ap => ap.Lesson!.StudyGroup!.SubjectID == SelectedSubject.SubjectID);
                }
                if (SelectedLesson != null)
                {
                    query = query.Where(ap => ap.LessonID == SelectedLesson.LessonID);
                }

                var performanceData = await query.ToListAsync();

                DiaryCollection.Clear();
                foreach (var item in performanceData.OrderByDescending(ap => ap.Lesson?.LessonDate).ThenByDescending(ap => ap.Lesson?.LessonTime))
                {
                    DiaryCollection.Add(new AcademicPerformanceDisplayModel
                    {
                        AcademicPerformanceId = item.PerformanceID,
                        StudentFullName = item.Student?.FullName,
                        GroupName = item.Student?.Group?.GroupName,
                        LessonDescription = item.Lesson?.Topic,
                        LessonDate = item.Lesson?.LessonDate ?? DateTime.MinValue,
                        LessonTime = item.Lesson?.LessonTime ?? TimeSpan.Zero,
                        SubjectName = item.Lesson?.StudyGroup?.Subject?.SubjectName,
                        Grade = item.Grade ?? "Н/А",
                        Comment = item.Comment,
                        StudentId = item.StudentID,
                        LessonId = item.LessonID,
                        GroupId = item.Student?.GroupID,
                        SubjectId = item.Lesson?.StudyGroup?.SubjectID
                    });
                }
            }
        }

        partial void OnSelectedGroupChanged(Group? value)
        {
            _ = LoadStudentsAndLessonsForGroupAsync(value);
            _ = LoadDiaryDataAsync();
        }

        partial void OnSelectedStudentChanged(User? value)
        {
            _ = LoadDiaryDataAsync();
        }

        partial void OnSelectedSubjectChanged(Subject? value)
        {
            if (SelectedGroup != null)
            {
                _ = LoadLessonsForGroupAndSubjectAsync(SelectedGroup, value);
            }
            _ = LoadDiaryDataAsync();
        }

        partial void OnSelectedLessonChanged(Lesson? value)
        {
            _ = LoadDiaryDataAsync();
        }

        internal async Task LoadStudentsAndLessonsForGroupAsync(Group? group)
        {
            StudentsInSelectedGroup.Clear();
            LessonsForSelectedStudent.Clear();
            SelectedStudent = null;
            SelectedLesson = null;

            if (group != null)
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var students = await dbContext.Users
                        .Where(u => u.RoleID == 3 && u.GroupID == group.GroupID)
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToListAsync();
                    StudentsInSelectedGroup.Clear();
                    foreach (var student in students)
                    {
                        StudentsInSelectedGroup.Add(student);
                    }

                    await LoadLessonsForGroupAndSubjectAsync(group, SelectedSubject);
                }
            }
        }

        internal async Task LoadLessonsForGroupAndSubjectAsync(Group? group, Subject? subject)
        {
            LessonsForSelectedStudent.Clear();
            SelectedLesson = null;

            if (group != null && _currentTeacherUser != null)
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var queryStudyGroups = dbContext.StudyGroups
                        .Where(sg => sg.GroupID == group.GroupID &&
                                     sg.TeacherID == _currentTeacherUser.UserID);

                    if (subject != null)
                    {
                        queryStudyGroups = queryStudyGroups.Where(sg => sg.SubjectID == subject.SubjectID);
                    }

                    var studyGroupIds = await queryStudyGroups.Select(sg => sg.StudyGroupID).ToListAsync();

                    if (!studyGroupIds.Any())
                    {
                        return;
                    }

                    var lessons = await dbContext.Lessons
                        .Where(l => studyGroupIds.Contains(l.StudyGroupID))
                        .OrderByDescending(l => l.LessonDate)
                        .ThenByDescending(l => l.LessonTime)
                        .ToListAsync();

                    LessonsForSelectedStudent.Clear();
                    foreach (var lesson in lessons)
                    {
                        LessonsForSelectedStudent.Add(lesson);
                    }
                }
            }
        }

        [RelayCommand]
        private async Task PerformGradeActionAsync()
        {
            if (SelectedActionType == null || SelectedStudent == null || SelectedLesson == null || SelectedSubject == null || SelectedGroup == null || SelectedGrade == null)
            {
                return;
            }

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var actualStudyGroup = await dbContext.StudyGroups
                    .FirstOrDefaultAsync(sg => sg.StudyGroupID == SelectedLesson.StudyGroupID &&
                                                sg.TeacherID == _currentTeacherUser!.UserID &&
                                                sg.SubjectID == SelectedSubject.SubjectID &&
                                                sg.GroupID == SelectedGroup.GroupID);

                if (actualStudyGroup == null)
                {
                    return;
                }

                var existingPerformance = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.StudentID == SelectedStudent.UserID &&
                                                ap.LessonID == SelectedLesson.LessonID);

                switch (SelectedActionType)
                {
                    case "Добавить":
                        if (existingPerformance != null)
                        {
                            return;
                        }

                        var newPerformance = new AcademicPerformance
                        {
                            StudentID = SelectedStudent.UserID,
                            LessonID = SelectedLesson.LessonID,
                            Grade = SelectedGrade == "Н/А" ? null : SelectedGrade,
                            Attendance = (SelectedGrade != "Н/А"),
                            Comment = CommentInput
                        };
                        dbContext.AcademicPerformance.Add(newPerformance);
                        break;

                    case "Обновить":
                        if (existingPerformance == null)
                        {
                            return;
                        }

                        existingPerformance.Grade = SelectedGrade == "Н/А" ? null : SelectedGrade;
                        existingPerformance.Attendance = (SelectedGrade != "Н/А");
                        existingPerformance.Comment = CommentInput;
                        break;

                    case "Удалить":
                        if (existingPerformance == null)
                        {
                            return;
                        }
                        dbContext.AcademicPerformance.Remove(existingPerformance);
                        break;
                }

                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();

                WeakReferenceMessenger.Default.Send(new GradesUpdatedMessage(true));

                ClearActionInputFields();
                SelectedStudent = null;
                SelectedLesson = null;
                SelectedSubject = null;
                SelectedGroup = null;

                await LoadDiaryDataAsync();
            }
        }

        private int _editingPerformanceId;

        [RelayCommand]
        private async Task EditGrade(AcademicPerformanceDisplayModel? performance)
        {
            ClearActionInputFields();
            _editingPerformanceId = 0;

            if (performance == null)
            {
                SelectedActionType = "Добавить";
                return;
            }

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var fullPerformance = await dbContext.AcademicPerformance
                    .Include(ap => ap.Student)
                    .Include(ap => ap.Lesson)
                        .ThenInclude(l => l!.StudyGroup)
                            .ThenInclude(sg => sg!.Subject)
                    .Include(ap => ap.Lesson)
                        .ThenInclude(l => l!.StudyGroup)
                            .ThenInclude(sg => sg!.Group)
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == performance.AcademicPerformanceId);

                if (fullPerformance == null || fullPerformance.Student == null || fullPerformance.Lesson == null || fullPerformance.Lesson.StudyGroup == null || fullPerformance.Lesson.StudyGroup.Subject == null || fullPerformance.Lesson.StudyGroup.Group == null)
                {
                    Debug.WriteLine("Error: Full performance data not found for edit or related entities are null.");
                    return;
                }

                SelectedActionType = "Обновить";

                SelectedGroup = Groups.FirstOrDefault(g => g.GroupID == fullPerformance.Lesson.StudyGroup.GroupID);

                if (SelectedGroup != null)
                {
                    await LoadStudentsAndLessonsForGroupAsync(SelectedGroup);
                }

                SelectedStudent = StudentsInSelectedGroup.FirstOrDefault(s => s.UserID == fullPerformance.StudentID);

                SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectID == fullPerformance.Lesson.StudyGroup.SubjectID);

                if (SelectedGroup != null && SelectedSubject != null)
                {
                    await LoadLessonsForGroupAndSubjectAsync(SelectedGroup, SelectedSubject);
                }

                SelectedLesson = LessonsForSelectedStudent.FirstOrDefault(l => l.LessonID == fullPerformance.LessonID);

                SelectedGrade = AvailableGrades.FirstOrDefault(g => (g == "Н/А" && fullPerformance.Grade == null) || g == fullPerformance.Grade);
                CommentInput = fullPerformance.Comment;

                _editingPerformanceId = performance.AcademicPerformanceId;
            }
        }

        [RelayCommand]
        private async Task DeleteGrade(AcademicPerformanceDisplayModel? performance)
        {
            if (performance == null) return;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var academicPerformanceToDelete = await dbContext.AcademicPerformance
                    .FirstOrDefaultAsync(ap => ap.PerformanceID == performance.AcademicPerformanceId);

                if (academicPerformanceToDelete != null)
                {
                    dbContext.AcademicPerformance.Remove(academicPerformanceToDelete);
                    await dbContext.SaveChangesAsync();
                    dbContext.ChangeTracker.Clear();

                    WeakReferenceMessenger.Default.Send(new GradesUpdatedMessage(true));
                    await LoadDiaryDataAsync();
                }
                ClearActionInputFields();
                SelectedStudent = null;
                SelectedLesson = null;
                SelectedSubject = null;
            }
        }

        private void ClearActionInputFields()
        {
            SelectedGrade = null;
            CommentInput = null;
            SelectedActionType = null;
        }

        private void ClearAllInputFields()
        {
            SelectedGroup = null;
            SelectedStudent = null;
            SelectedLesson = null;
            SelectedSubject = null;
            SelectedGrade = null;
            CommentInput = null;
            SelectedActionType = null;
            StudentsInSelectedGroup.Clear();
            LessonsForSelectedStudent.Clear();
        }
    }
}