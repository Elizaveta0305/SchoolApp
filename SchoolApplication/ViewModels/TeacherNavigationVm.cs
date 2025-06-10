using SchoolApplication.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolApplication.ViewModels;
using System.Windows.Input;
using SchoolApplication.Views.UserControls.TeacherUC;

namespace SchoolApplication.ViewModels
{
    public class TeacherNavigationVm : ViewModelBase
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
