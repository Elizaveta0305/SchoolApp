using CommunityToolkit.Mvvm.ComponentModel;
using SchoolApplication.Utilities;
using SchoolApplication.ViewModels;
using System.Windows.Input;

namespace SchoolApplication.ViewModels
{
     public class NavigationVm : ObservableObject
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

        public ICommand HomeCommand { get; }
        public ICommand LessonsCommand { get; }
        public ICommand GradeCommand { get; }

        public NavigationVm()
        {
            HomeCommand = new RelayCommand(_ => ShowHome());
            LessonsCommand = new RelayCommand(_ => ShowLessons());
            GradeCommand = new RelayCommand(_ => ShowGrades());

            ShowHome();
        }

        private void ShowHome() => CurrentView = new HomeVm();
        private void ShowLessons() => CurrentView = new LessonsVm();
        private void ShowGrades() => CurrentView = new GradeVm();
    }
}