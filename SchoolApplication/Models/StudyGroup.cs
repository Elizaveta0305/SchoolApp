using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class StudyGroup
    {
        public int StudyGroupId { get; set; }
        public string AcademicYear { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int TeacherId { get; set; }
        public User Teacher { get; set; }

        public ICollection<AcademicPerformance> AcademicPerformances { get; set; }
    }
}
