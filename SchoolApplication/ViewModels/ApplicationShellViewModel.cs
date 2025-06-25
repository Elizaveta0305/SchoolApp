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

        private IMessenger _messenger;

        [ObservableProperty]
        private ObservableObject _currentMainContentViewModel;

        private User _authenticatedUser;

        // ViewModel'ы для админской части
        private HomeAdminVm _homeAdminVm;
        private ClassroomsAdminVm _classroomsAdminVm;
        private DiaryAdminVm _diaryAdminVm;
        private GroupsAdminVm _groupsAdminVm;
        private SubjectAdminVm _subjectsAdminVm;
        private UsersAdminVm _usersAdminVm;

        // ViewModel'ы для студенческой части
        private HomeVm _homeStudentVm;
        private GradeVm _gradeVm;
        private LessonsVm _lessonsVm;

        // ViewModel'ы для учительской части
        private HomeTeacherVm _homeTeacherVm;
        private DiaryTeacherVm _diaryTeacherVm;
        private LessonTeacherVm _lessonTeacherVm;

        // ViewModel'ы для навигационных панелей
        private NavigationAdminVm _navigationAdminVm;
        private NavigationVm _navigationVm; // Для студента
        private TeacherNavigationVm _teacherNavigationVm;

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
            TeacherNavigationVm teacherNavigationVm,
            IMessenger messenger)
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

            _messenger = messenger;

            _messenger.Register<ApplicationShellViewModel, NavigateMessage>(this, (r, m) => r.Receive(m));

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