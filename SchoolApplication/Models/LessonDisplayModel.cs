using System;
using System.ComponentModel;
using System.Globalization;

namespace SchoolApplication.Models
{
    public class LessonDisplayModel : INotifyPropertyChanged
    {
        private string _subjectName;
        public string SubjectName
        {
            get => _subjectName;
            set
            {
                if (_subjectName != value)
                {
                    _subjectName = value;
                    OnPropertyChanged(nameof(SubjectName));
                }
            }
        }

        private string _teacherFullName;
        public string TeacherFullName
        {
            get => _teacherFullName;
            set
            {
                if (_teacherFullName != value)
                {
                    _teacherFullName = value;
                    OnPropertyChanged(nameof(TeacherFullName));
                }
            }
        }

        private string _roomNumber;
        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                if (_roomNumber != value)
                {
                    _roomNumber = value;
                    OnPropertyChanged(nameof(RoomNumber));
                }
            }
        }

        private DateOnly _lessonDate;
        public DateOnly LessonDate
        {
            get => _lessonDate;
            set
            {
                if (_lessonDate != value)
                {
                    _lessonDate = value;
                    OnPropertyChanged(nameof(LessonDate));
                    OnPropertyChanged(nameof(FormattedLessonDate));
                }
            }
        }

        private TimeSpan _lessonTime;
        public TimeSpan LessonTime
        {
            get => _lessonTime;
            set
            {
                if (_lessonTime != value)
                {
                    _lessonTime = value;
                    OnPropertyChanged(nameof(LessonTime));
                    OnPropertyChanged(nameof(FormattedLessonTime));
                }
            }
        }

        public string FormattedLessonDate => LessonDate.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        public string FormattedLessonTime => LessonTime.ToString(@"hh\:mm", CultureInfo.CurrentCulture);
        public DateTime FullLessonDateTime { get; set; }

        public int LessonId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}