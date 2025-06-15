using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class Lesson
    {
        [Key]
        public int LessonID { get; set; }
        public DateTime LessonDate { get; set; }
        public TimeSpan LessonTime { get; set; }
        public string? Topic { get; set; }

        public int StudyGroupID { get; set; }
        public int? ClassroomID { get; set; }

        [ForeignKey("StudyGroupID")]
        public StudyGroup? StudyGroup { get; set; }
        [ForeignKey("ClassroomID")]
        public Classroom? Classroom { get; set; }

        public ICollection<AcademicPerformance>? AcademicPerformances { get; set; }
    }
}
