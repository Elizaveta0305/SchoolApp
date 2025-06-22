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
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;


namespace SchoolApplication.ViewModels
{
    public partial class MainViewModel : ObservableObject,
        IRecipient<UserAuthenticatedMessage>
    {
        [ObservableProperty]
        private ObservableObject _currentApplicationContent;

        private readonly LoginViewModel _loginViewModel;

        private readonly HomeAdminVm _homeAdminVm;
        private readonly HomeTeacherVm _homeTeacherVm;
        private readonly HomeVm _homeStudentVm;

        private readonly ClassroomsAdminVm _classroomsAdminVm;
        private readonly DiaryAdminVm _diaryAdminVm;
        private readonly GroupsAdminVm _groupsAdminVm;
        private readonly SubjectAdminVm _subjectsAdminVm;
        private readonly UsersAdminVm _usersAdminVm;

        private readonly GradeVm _gradeVm;
        private readonly LessonsVm _lessonsVm;

        private readonly DiaryTeacherVm _diaryTeacherVm;
        private readonly LessonTeacherVm _lessonTeacherVm;

        private readonly NavigationAdminVm _navigationAdminVm;
        private readonly NavigationVm _navigationVm;
        private readonly TeacherNavigationVm _teacherNavigationVm;
        private readonly IMessenger _messenger;


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
            TeacherNavigationVm teacherNavigationVm,
            IMessenger messenger)
        {
            _messenger = messenger;

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

            _messenger.Register<MainViewModel, UserAuthenticatedMessage>(this, (r, m) => r.Receive(m));

            CurrentApplicationContent = _loginViewModel;
        }

        public void Receive(UserAuthenticatedMessage message)
        {
            if (message?.Value != null)
            {
                CurrentApplicationContent = new ApplicationShellViewModel(
                    message.Value,
                    _homeStudentVm, _homeAdminVm, _homeTeacherVm,
                    _classroomsAdminVm, _diaryAdminVm, _groupsAdminVm, _subjectsAdminVm, _usersAdminVm,
                    _gradeVm, _lessonsVm,
                    _diaryTeacherVm, _lessonTeacherVm,
                    _navigationAdminVm, _navigationVm, _teacherNavigationVm,
                    _messenger
                );
            }
            else
            {
                CurrentApplicationContent = _loginViewModel;
                _loginViewModel.Username = string.Empty;
                _loginViewModel.Password = string.Empty;
                _loginViewModel.ErrorMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _messenger.Send(new UserAuthenticatedMessage(null));
        }
    }
}