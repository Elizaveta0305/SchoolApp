using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchoolApplication.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleID { get; set; }
        public int? GroupID { get; set; }

        [ForeignKey("RoleID")]
        public Role? Role { get; set; }
        [ForeignKey("GroupID")]
        public Group? Group { get; set; }

        public ICollection<StudyGroup>? StudyGroupsAsTeacher { get; set; }
        public ICollection<AcademicPerformance>? AcademicPerformanceAsStudent { get; set; }
    }
}
