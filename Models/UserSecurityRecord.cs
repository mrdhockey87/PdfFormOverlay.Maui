using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Models
{
    [Table("UserSecurity")]
    public class UserSecurityRecord
    {
        [PrimaryKey]
        public int Id { get; set; } = 1; // Single record

        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string SecurityQuestion1 { get; set; }
        public string SecurityQuestion2 { get; set; }
        public string SecurityQuestion3 { get; set; }
        public string SecurityAnswer1Hash { get; set; }
        public string SecurityAnswer2Hash { get; set; }
        public string SecurityAnswer3Hash { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastPasswordChange { get; set; }
    }
}