using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SchoolApplication.ViewModels.LessonTeacherVm;

namespace SchoolApplication.Models
{
    public class StudyGroup
    {
        [Key]
        public int StudyGroupID { get; set; }
        public string AcademicYear { get; set; } = string.Empty;

        public int GroupID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }

        [ForeignKey("GroupID")]
        public Group? Group { get; set; }
        [ForeignKey("SubjectID")]
        public Subject? Subject { get; set; }
        [ForeignKey("TeacherID")]
        public User? Teacher { get; set; }

        public ICollection<Lesson>? Lessons { get; set; }
    }
}
