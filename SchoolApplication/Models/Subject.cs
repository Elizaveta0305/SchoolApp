using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class Subject
    {
        [Key]
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<StudyGroup>? StudyGroups { get; set; }
    }
}
