using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Models
{
    // Database Models
    [Table("FormData")]
    public class FormDataRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FormId { get; set; }
        public string FormName { get; set; }
        public string EncryptedFieldData { get; set; } // JSON encrypted with AES
        public DateTime SavedDate { get; set; }
        public DateTime LastModified { get; set; }
    }
}
