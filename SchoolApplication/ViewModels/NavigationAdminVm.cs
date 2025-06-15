using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Messages;
using SchoolApplication.ViewModels;
using System.Windows.Input;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class NavigationAdminVm : ObservableObject
    {

        private readonly HomeAdminVm _homeAdminVm;
        private readonly LessonAdminVm _lessonsAdminVm;
        private readonly DiaryAdminVm _diaryAdminVm;
        private readonly ClassroomsAdminVm _classroomsAdminVm;
        private readonly SubjectAdminVm _subjectAdminVm;
        private readonly UsersAdminVm _usersAdminVm;
        private readonly GroupsAdminVm _groupsAdminVm;

        public ICommand HomeAdminCommand { get; }
        public ICommand LessonsAdminCommand { get; }
        public ICommand DiaryAdminCommand { get; }
        public ICommand ClassroomsAdminCommand { get; }
        public ICommand SubjectAdminCommand { get; }
        public ICommand UsersAdminCommand { get; }
        public ICommand GroupsAdminCommand { get; }

        public NavigationAdminVm(
            HomeAdminVm homeAdminVm,
            LessonAdminVm lessonsAdminVm,
            DiaryAdminVm diaryAdminVm,
            ClassroomsAdminVm classroomsAdminVm,
            SubjectAdminVm subjectAdminVm,
            UsersAdminVm usersAdminVm,
            GroupsAdminVm groupsAdminVm)
        {
            _homeAdminVm = homeAdminVm;
            _lessonsAdminVm = lessonsAdminVm;
            _diaryAdminVm = diaryAdminVm;
            _classroomsAdminVm = classroomsAdminVm;
            _subjectAdminVm = subjectAdminVm;
            _usersAdminVm = usersAdminVm;
            _groupsAdminVm = groupsAdminVm;

            HomeAdminCommand = new RelayCommand(ExecuteHomeAdminCommand);
            LessonsAdminCommand = new RelayCommand(ExecuteLessonsAdminCommand);
            DiaryAdminCommand = new RelayCommand(ExecuteDiaryAdminCommand);
            ClassroomsAdminCommand = new RelayCommand(ExecuteClassroomsAdminCommand);
            SubjectAdminCommand = new RelayCommand(ExecuteSubjectAdminCommand);
            UsersAdminCommand = new RelayCommand(ExecuteUsersAdminCommand);
            GroupsAdminCommand = new RelayCommand(ExecuteGroupsAdminCommand);

        }

        private void ExecuteHomeAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: HomeAdminCommand executed. Sending NavigateMessage for HomeAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_homeAdminVm));
        }

        private void ExecuteLessonsAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: LessonsAdminCommand executed. Sending NavigateMessage for LessonsAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_lessonsAdminVm));
        }

        private void ExecuteDiaryAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: DiaryAdminCommand executed. Sending NavigateMessage for DiaryAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_diaryAdminVm));
        }

        private void ExecuteClassroomsAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: ClassroomsAdminCommand executed. Sending NavigateMessage for ClassroomsAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_classroomsAdminVm));
        }

        private void ExecuteSubjectAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: SubjectAdminCommand executed. Sending NavigateMessage for SubjectAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_subjectAdminVm));
        }

        private void ExecuteUsersAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: UsersAdminCommand executed. Sending NavigateMessage for UsersAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_usersAdminVm));
        }

        private void ExecuteGroupsAdminCommand()
        {
            Debug.WriteLine("NavigationAdminVm: GroupsAdminCommand executed. Sending NavigateMessage for GroupsAdminVm.");
            WeakReferenceMessenger.Default.Send(new NavigateMessage(_groupsAdminVm));
        }
    }
}