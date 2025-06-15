using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Messages;
using System.Windows.Input;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class NavigationVm : ObservableObject
    {
        private readonly HomeVm _homeVm;
        private readonly LessonsVm _lessonsVm;
        private readonly GradeVm _gradeVm;

        public ICommand HomeCommand { get; }
        public ICommand LessonsCommand { get; }
        public ICommand GradeCommand { get; }

        public NavigationVm(HomeVm homeVm, LessonsVm lessonsVm, GradeVm gradeVm)
        {
            _homeVm = homeVm;
            _lessonsVm = lessonsVm;
            _gradeVm = gradeVm;

            HomeCommand = new RelayCommand(ExecuteHomeCommand);
            LessonsCommand = new RelayCommand(ExecuteLessonsCommand);
            GradeCommand = new RelayCommand(ExecuteGradeCommand);

        }

        private void ExecuteHomeCommand()
        {
            Debug.WriteLine("NavigationVm: HomeCommand executed. Sending NavigateMessage for HomeVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_homeVm));
        }

        private void ExecuteLessonsCommand()
        {
            Debug.WriteLine("NavigationVm: LessonsCommand executed. Sending NavigateMessage for LessonsVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_lessonsVm));
        }

        private void ExecuteGradeCommand()
        {
            Debug.WriteLine("NavigationVm: GradeCommand executed. Sending NavigateMessage for GradeVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_gradeVm));
        }
    }
}