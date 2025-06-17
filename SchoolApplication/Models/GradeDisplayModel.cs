using System;

namespace SchoolApplication.Models
{
    public class GradeDisplayModel
    {
        public int PerformanceID { get; set; }
        public string SubjectName { get; set; }
        public string TeacherFullName { get; set; }
        public DateOnly LessonDate { get; set; }
        public TimeSpan LessonTime { get; set; }
        public string GradeValue { get; set; }
        public bool AttendanceMark { get; set; }
        public string Comment { get; set; }

        public string DateTimeDisplay => $"{LessonDate.ToShortDateString()} {LessonTime:hh\\:mm}";
    }
}