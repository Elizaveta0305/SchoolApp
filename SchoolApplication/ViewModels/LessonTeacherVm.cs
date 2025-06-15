using CommunityToolkit.Mvvm.ComponentModel;
using SchoolApplication.Views.UserControls.StudentUC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.ViewModels
{
    public class LessonTeacherVm : ObservableObject
    {

        private ObservableCollection<Lesson> _lessonsCollection;

        public ObservableCollection<Lesson> LessonsCollection
        {
            get => _lessonsCollection;
            set
            {
                _lessonsCollection = value;
                OnPropertyChanged();
            }
        }

        public LessonTeacherVm()
        {
            LessonsCollection = new ObservableCollection<Lesson>
            {
                new Lesson { Group = "Группа 1", Subject = "3d design", Time = new System.TimeSpan(9, 0, 0) },
                new Lesson { Group = "Группа 2", Subject = "Алгоритмика", Time = new System.TimeSpan(11, 30, 0) },
                new Lesson { Group = "Группа 3", Subject = "БПЛА", Time = new System.TimeSpan(14, 0, 0) },
            };
        }

        public class Lesson
        {
            public string Group { get; set; }
            public string Subject { get; set; }
            public System.TimeSpan Time { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
