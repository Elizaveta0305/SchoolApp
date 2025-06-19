using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Models;
using SchoolApplication.Messages;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class ApplicationShellViewModel : ObservableObject, IRecipient<NavigateMessage>
    {
        [ObservableProperty]
        private ObservableObject? _currentNavigationViewModel;

        [ObservableProperty]
        private ObservableObject _currentMainContentViewModel;

        private readonly User _authenticatedUser;

        // ViewModel'ы для админской части
        private readonly HomeAdminVm _homeAdminVm;
        private readonly ClassroomsAdminVm _classroomsAdminVm;
        private readonly DiaryAdminVm _diaryAdminVm;
        private readonly GroupsAdminVm _groupsAdminVm;
        private readonly SubjectAdminVm _subjectsAdminVm;
        private readonly UsersAdminVm _usersAdminVm;

        // ViewModel'ы для студенческой части
        private readonly HomeVm _homeStudentVm;
        private readonly GradeVm _gradeVm;
        private readonly LessonsVm _lessonsVm;

        // ViewModel'ы для учительской части
        private readonly HomeTeacherVm _homeTeacherVm;
        private readonly DiaryTeacherVm _diaryTeacherVm;
        private readonly LessonTeacherVm _lessonTeacherVm;

        // ViewModel'ы для навигационных панелей
        private readonly NavigationAdminVm _navigationAdminVm;
        private readonly NavigationVm _navigationVm; // Для студента
        private readonly TeacherNavigationVm _teacherNavigationVm;

        public ApplicationShellViewModel(
            User authenticatedUser,
            HomeVm homeStudentVm,
            HomeAdminVm homeAdminVm,
            HomeTeacherVm homeTeacherVm,
            ClassroomsAdminVm classroomsAdminVm,
            DiaryAdminVm diaryAdminVm,
            GroupsAdminVm groupsAdminVm,
            SubjectAdminVm subjectsAdminVm,
            UsersAdminVm usersAdminVm,
            GradeVm gradeVm,
            LessonsVm lessonsVm,
            DiaryTeacherVm diaryTeacherVm,
            LessonTeacherVm lessonTeacherVm,
            NavigationAdminVm navigationAdminVm,
            NavigationVm navigationVm,
            TeacherNavigationVm teacherNavigationVm)
        {
            _authenticatedUser = authenticatedUser;

            _homeStudentVm = homeStudentVm;
            _homeAdminVm = homeAdminVm;
            _homeTeacherVm = homeTeacherVm;

            _classroomsAdminVm = classroomsAdminVm;
            _diaryAdminVm = diaryAdminVm;
            _groupsAdminVm = groupsAdminVm;
            _subjectsAdminVm = subjectsAdminVm;
            _usersAdminVm = usersAdminVm;

            _gradeVm = gradeVm;
            _lessonsVm = lessonsVm;

            _diaryTeacherVm = diaryTeacherVm;
            _lessonTeacherVm = lessonTeacherVm;

            _navigationAdminVm = navigationAdminVm;
            _navigationVm = navigationVm;
            _teacherNavigationVm = teacherNavigationVm;

            WeakReferenceMessenger.Default.Register<NavigateMessage>(this);

            InitializeShellContent();
        }

        private void InitializeShellContent()
        {
            switch (_authenticatedUser.Role?.RoleName)
            {
                case "Администратор":
                    CurrentMainContentViewModel = _homeAdminVm;
                    CurrentNavigationViewModel = _navigationAdminVm;
                    break;
                case "Преподаватель":
                    CurrentMainContentViewModel = _homeTeacherVm;
                    CurrentNavigationViewModel = _teacherNavigationVm;
                    break;
                case "Ученик":
                    CurrentMainContentViewModel = _homeStudentVm;
                    CurrentNavigationViewModel = _navigationVm;
                    break;
                default:
                    break;
            }
        }

        public void Receive(NavigateMessage message)
        {
            if (message?.Value != null)
            {
                CurrentMainContentViewModel = message.Value switch
                {
                    // Админ
                    HomeAdminVm => _homeAdminVm,
                    ClassroomsAdminVm => _classroomsAdminVm,
                    DiaryAdminVm => _diaryAdminVm,
                    GroupsAdminVm => _groupsAdminVm,
                    SubjectAdminVm => _subjectsAdminVm,
                    UsersAdminVm => _usersAdminVm,
                    // Ученик
                    HomeVm => _homeStudentVm,
                    GradeVm => _gradeVm,
                    LessonsVm => _lessonsVm,
                    // Учитель
                    HomeTeacherVm => _homeTeacherVm,
                    DiaryTeacherVm => _diaryTeacherVm,
                    LessonTeacherVm => _lessonTeacherVm,
                    _ => CurrentMainContentViewModel
                };
            }
        }
    }
}