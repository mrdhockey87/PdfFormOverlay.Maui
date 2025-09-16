using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Models
{
    // Core Models (unchanged from previous version)
    public class FormField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int PageNumber { get; set; }
        public bool IsRequired { get; set; }
        public string[] Options { get; set; }
    }
}
