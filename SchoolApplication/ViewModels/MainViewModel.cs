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

namespace SchoolApplication.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentMainContentViewModel;

        [ObservableProperty]
        private ObservableObject? _currentNavigationViewModel;

        [ObservableProperty]
        private User? _currentUser;

        // Объявление readonly полей для всех ViewModel, которые MainViewModel будет переключать
        private readonly LoginViewModel _loginViewModel;
        private readonly HomeAdminVm _homeAdminVm;
        private readonly HomeTeacherVm _homeTeacherVm;
        private readonly HomeVm _homeStudentVm;

        // ViewModel'ы для админской части (примеры)
        private readonly ClassroomsAdminVm _classroomsAdminVm;
        private readonly DiaryAdminVm _diaryAdminVm;
        private readonly GroupsAdminVm _groupsAdminVm;
        private readonly SubjectAdminVm _subjectsAdminVm;
        private readonly UsersAdminVm _usersAdminVm;

        // ViewModel'ы для студенческой части (примеры)
        private readonly GradeVm _gradeVm; // Добавьте, если используете Grade.xaml
        private readonly LessonsVm _lessonsVm; // Добавьте, если используете Lessons.xaml

        // ViewModel'ы для учительской части (примеры)
        private readonly DiaryTeacherVm _diaryTeacherVm; // Добавьте, если используете DiaryTeacherView.xaml
        private readonly LessonTeacherVm _lessonTeacherVm; // Добавьте, если используете LessonTeacherView.xaml

        // ViewModel'ы для навигационных панелей
        private readonly NavigationAdminVm _navigationAdminVm;
        private readonly NavigationVm _navigationVm; // Для студента
        private readonly TeacherNavigationVm _teacherNavigationVm;


        public MainViewModel(
            LoginViewModel loginViewModel,
            HomeVm homeStudentVm,
            HomeAdminVm homeAdminVm,
            HomeTeacherVm homeTeacherVm,

            // Админские ViewModel
            ClassroomsAdminVm classroomsAdminVm,
            DiaryAdminVm diaryAdminVm,
            GroupsAdminVm groupsAdminVm,
            SubjectAdminVm subjectsAdminVm,
            UsersAdminVm usersAdminVm,

            // Студенческие ViewModel
            GradeVm gradeVm,
            LessonsVm lessonsVm,

            // Учительские ViewModel
            DiaryTeacherVm diaryTeacherVm,
            LessonTeacherVm lessonTeacherVm,

            // Навигационные ViewModel
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

            WeakReferenceMessenger.Default.Register<UserAuthenticatedMessage>(this, (object recipient, UserAuthenticatedMessage message) =>
            {
                MainViewModel r = (MainViewModel)recipient;
                r.CurrentUser = message.Value;
                r.NavigateBasedOnRole(message.Value.Role?.RoleName);
            });
        }

        public void NavigateBasedOnRole(string? roleName)
        {
            switch (roleName)
            {
                case "Администратор":
                    Console.WriteLine("Навигация: Администратор.");
                    CurrentMainContentViewModel = _homeAdminVm; //
                    CurrentNavigationViewModel = _navigationAdminVm; //
                    break;
                case "Преподаватель":
                    Console.WriteLine("Навигация: Учитель.");
                    CurrentMainContentViewModel = _homeTeacherVm; //
                    CurrentNavigationViewModel = _teacherNavigationVm; //
                    break;
                case "Ученик":
                    Console.WriteLine("Навигация: Студент.");
                    CurrentMainContentViewModel = _homeStudentVm; //
                    CurrentNavigationViewModel = _navigationVm; //
                    break;
                default:
                    Console.WriteLine("Навигация: Неизвестная роль или выход. Возврат к логину.");
                    CurrentMainContentViewModel = _loginViewModel;
                    CurrentNavigationViewModel = null;
                    CurrentUser = null;
                    break;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            NavigateBasedOnRole(null);
            Console.WriteLine("Пользователь вышел из системы.");
        }
    }
}