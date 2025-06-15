using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class Group
    {
        [Key]
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<User>? Users { get; set; }
        public ICollection<StudyGroup>? StudyGroups { get; set; }
    }
}
