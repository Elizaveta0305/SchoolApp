using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SchoolApplication.ViewModels.LessonTeacherVm;

namespace SchoolApplication.Models
{
    public class Classroom
    {
        [Key]
        public int ClassroomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;

        public ICollection<Lesson>? Lessons { get; set; }
    }
}
