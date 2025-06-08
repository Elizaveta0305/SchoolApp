using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class AcademicPerformance
    {
        public int PerformanceId { get; set; }
        public DateTime GradeDate { get; set; }
        public int? Grade { get; set; }
        public bool Attendance { get; set; }
        public string Comment { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public int StudyGroupId { get; set; }
        public StudyGroup StudyGroup { get; set; }
    }
}
