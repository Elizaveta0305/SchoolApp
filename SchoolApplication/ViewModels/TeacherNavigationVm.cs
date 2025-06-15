using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Messages;
using SchoolApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SchoolApplication.ViewModels
{
    public partial class TeacherNavigationVm : ObservableObject
    {
        private readonly HomeTeacherVm _homeTeacherVm;
        private readonly LessonTeacherVm _lessonsTeacherViewModel;
        private readonly DiaryTeacherVm _diaryTeacherViewModel;

        public TeacherNavigationVm(
            HomeTeacherVm homeTeacherVm,
            LessonTeacherVm lessonsTeacherViewModel,
            DiaryTeacherVm diaryTeacherViewModel)
        {
            _homeTeacherVm = homeTeacherVm;
            _lessonsTeacherViewModel = lessonsTeacherViewModel;
            _diaryTeacherViewModel = diaryTeacherViewModel;

            HomeTeacherCommand = new RelayCommand(ExecuteHomeTeacherCommand);
            LessonsTeacherCommand = new RelayCommand(ExecuteLessonsTeacherCommand);
            DiaryTeacherCommand = new RelayCommand(ExecuteDiaryTeacherCommand);
        }

        private void ExecuteHomeTeacherCommand()
        {
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_homeTeacherVm));
        }

        private void ExecuteLessonsTeacherCommand()
        {
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_lessonsTeacherViewModel));
        }

        private void ExecuteDiaryTeacherCommand()
        {
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_diaryTeacherViewModel));
        }

        public ICommand HomeTeacherCommand { get; }
        public ICommand LessonsTeacherCommand { get; }
        public ICommand DiaryTeacherCommand { get; }
    }
}