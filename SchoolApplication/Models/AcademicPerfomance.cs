using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class AcademicPerformance
    {
        [Key]
        public int PerformanceID { get; set; }
        public string? Grade { get; set; }
        public bool Attendance { get; set; }
        public string? Comment { get; set; }

        public int StudentID { get; set; }
        public int LessonID { get; set; }

        [ForeignKey("StudentID")]
        public User? Student { get; set; }
        [ForeignKey("LessonID")]
        public Lesson? Lesson { get; set; }
    }
}
