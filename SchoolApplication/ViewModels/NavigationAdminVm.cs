using SchoolApplication.Views.UserControls.TeacherUC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolApplication.ViewModels;
using SchoolApplication.Utilities;
using System.Windows.Input;
using SchoolApplication.Views.UserControls.AdminUC;

namespace SchoolApplication.ViewModels
{
    public class NavigationAdminVm : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand HomeAdminCommand { get; }
        public ICommand LessonsAdminCommand { get; }
        public ICommand DiaryAdminCommand { get; }
        public ICommand ClassroomsAdminCommand { get; }
        public ICommand SubjectAdminCommand { get; }
        public ICommand UsersAdminCommand { get; }
        public ICommand GroupsAdminCommand { get; }

        public NavigationAdminVm()
        {
            HomeAdminCommand = new RelayCommand(_ => ShowHome());
            LessonsAdminCommand = new RelayCommand(_ => ShowLessons());
            DiaryAdminCommand = new RelayCommand(_ => ShowDiary());
            ClassroomsAdminCommand = new RelayCommand(_ => ShowClassroom());
            SubjectAdminCommand = new RelayCommand(_ => ShowSubject());
            UsersAdminCommand = new RelayCommand(_ => ShowUser());
            GroupsAdminCommand = new RelayCommand(_ => ShowGroups());

            ShowHome();
        }

        private void ShowHome() => CurrentView = new HomeAdminView();
        private void ShowLessons() => CurrentView = new LessonsAdminView();
        private void ShowDiary() => CurrentView = new DiaryAdminView();
        private void ShowClassroom() => CurrentView = new ClassroomsAdminView();
        private void ShowSubject() => CurrentView = new SubjectAdminView();
        private void ShowUser() => CurrentView = new UsersAdminView();
        private void ShowGroups() => CurrentView = new GroupsAdminView();
    }
}

