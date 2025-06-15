using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolApplication.Models;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Messages;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class MainViewModel : ObservableObject,
        IRecipient<UserAuthenticatedMessage>,
        IRecipient<NavigateMessage>      
    {
        [ObservableProperty]
        private ObservableObject _currentMainContentViewModel;

        [ObservableProperty]
        private ObservableObject? _currentNavigationViewModel;

        [ObservableProperty]
        private User? _currentUser;

        private readonly LoginViewModel _loginViewModel;
        private readonly HomeAdminVm _homeAdminVm;
        private readonly HomeTeacherVm _homeTeacherVm;
        private readonly HomeVm _homeStudentVm;

        // ViewModel'ы для админской части
        private readonly ClassroomsAdminVm _classroomsAdminVm;
        private readonly DiaryAdminVm _diaryAdminVm;
        private readonly GroupsAdminVm _groupsAdminVm;
        private readonly SubjectAdminVm _subjectsAdminVm;
        private readonly UsersAdminVm _usersAdminVm;

        // ViewModel'ы для студенческой части
        private readonly GradeVm _gradeVm;
        private readonly LessonsVm _lessonsVm;

        // ViewModel'ы для учительской части
        private readonly DiaryTeacherVm _diaryTeacherVm;
        private readonly LessonTeacherVm _lessonTeacherVm;

        // ViewModel'ы для навигационных панелей
        private readonly NavigationAdminVm _navigationAdminVm;
        private readonly NavigationVm _navigationVm; // Для студента
        private readonly TeacherNavigationVm _teacherNavigationVm;


        public MainViewModel(
            LoginViewModel loginViewModel,
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
            _loginViewModel = loginViewModel;
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

            CurrentMainContentViewModel = _loginViewModel;
            CurrentNavigationViewModel = null;
            CurrentUser = null;

            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this);
            WeakReferenceMessenger.Default.Register<NavigateMessage>(this);
        }

        public void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                CurrentUser = message.Value;

                Debug.WriteLine($"User authenticated: {CurrentUser.Username}, Role: {CurrentUser.Role?.RoleName}");

                switch (CurrentUser.Role?.RoleName)
                {
                    case "Администратор":
                        Debug.WriteLine("Навигация: Администратор.");
                        CurrentMainContentViewModel = _homeAdminVm;
                        CurrentNavigationViewModel = _navigationAdminVm;
                        break;
                    case "Преподаватель":
                        Debug.WriteLine("Навигация: Учитель.");
                        CurrentMainContentViewModel = _homeTeacherVm;
                        CurrentNavigationViewModel = _teacherNavigationVm;
                        break;
                    case "Ученик":
                        Debug.WriteLine("Навигация: Студент.");
                        CurrentMainContentViewModel = _homeStudentVm;
                        CurrentNavigationViewModel = _navigationVm;
                        break;
                    default:
                        Debug.WriteLine("Навигация: Неизвестная роль или выход. Возврат к логину.");
                        CurrentMainContentViewModel = _loginViewModel;
                        CurrentNavigationViewModel = null;
                        CurrentUser = null;
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Сообщение об аутентификации пользователя было пустым или null. Возврат к логину.");
                CurrentMainContentViewModel = _loginViewModel;
                CurrentNavigationViewModel = null;
                CurrentUser = null;
            }
        }

        public void Receive(NavigateMessage message)
        {
            if (message?.Value != null)
            {
                CurrentMainContentViewModel = message.Value;
                Debug.WriteLine($"MainViewModel received NavigateMessage. Navigating to: {message.Value.GetType().Name}");
            }
        }

        [RelayCommand]
        private void Logout()
        {
            WeakReferenceMessenger.Default.Send(new UserAuthenticatedMessage(null));
            Debug.WriteLine("Пользователь вышел из системы.");
        }
    }
}