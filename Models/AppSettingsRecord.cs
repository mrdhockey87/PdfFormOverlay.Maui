using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Models
{
    [Table("AppSettings")]
    public class AppSettingsRecord
    {
        [PrimaryKey]
        public string Key { get; set; }

        public string Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
