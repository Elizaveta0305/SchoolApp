using System;
using SchoolApplication.Models;

namespace SchoolApplication.Models.DisplayModels
{
    public class LessonTeacherDisplayModel
    {
        public int LessonId { get; set; }
        public string GroupName { get; set; }
        public string SubjectName { get; set; }
        public DateTime LessonDate { get; set; }
        public TimeSpan LessonTime { get; set; }
        public string Topic { get; set; }
        public string? ClassroomNumber { get; set; }

        public string DateTimeDisplay => $"{LessonDate:dd.MM.yyyy} {LessonTime:hh\\:mm}";
    }
}