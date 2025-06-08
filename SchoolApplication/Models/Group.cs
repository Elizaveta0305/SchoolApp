using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<StudyGroup> StudyGroups { get; set; }
    }
}
