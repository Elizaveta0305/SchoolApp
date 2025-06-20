using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public partial class AcademicPerformanceDisplayModel : ObservableObject
    {
        public int AcademicPerformanceId { get; set; }
        public string? StudentFullName { get; set; }
        public string? GroupName { get; set; }
        public string? SubjectName { get; set; }
        public string? LessonDescription { get; set; }
        public DateTime LessonDate { get; set; }
        public TimeSpan LessonTime { get; set; }
        public string? Grade { get; set; }
        public string? Comment { get; set; }
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public int? GroupId { get; set; }
        public int? SubjectId { get; set; }
    }
}
