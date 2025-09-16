using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Models
{
    public class SavedFormData
    {
        public string FormId { get; set; }
        public string FormName { get; set; }
        public Dictionary<string, object> FieldValues { get; set; }
        public DateTime SavedDate { get; set; }
    }

}