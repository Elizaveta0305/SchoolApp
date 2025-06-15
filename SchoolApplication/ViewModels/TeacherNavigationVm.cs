using CommunityToolkit.Mvvm.ComponentModel;
using SchoolApplication.Utilities;
using SchoolApplication.ViewModels;
using SchoolApplication.Views.UserControls.TeacherUC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SchoolApplication.ViewModels
{
    public class TeacherNavigationVm : ObservableObject
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
        
        public ICommand HomeTeacherCommand { get; }
        public ICommand LessonsTeacherCommand { get; }
        public ICommand DiaryTeacherCommand { get; }

        public TeacherNavigationVm()
        {
            HomeTeacherCommand = new RelayCommand(_ => ShowHome());
            LessonsTeacherCommand = new RelayCommand(_ => ShowLessons());
            DiaryTeacherCommand = new RelayCommand(_ => ShowDiary());

            ShowHome();
        }

        private void ShowHome() => CurrentView = new HomeTeacherView();
        private void ShowLessons() => CurrentView = new LessonTeacherView();
        private void ShowDiary() => CurrentView = new DiaryTeacherView();
    }

}
