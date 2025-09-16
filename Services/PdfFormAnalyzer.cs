using iTextSharp.text.pdf;
using PdfFormOverlay.Maui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // PDF Form Analyzer (unchanged from previous version)
    public class PdfFormAnalyzer
    {
        public async Task<List<FormField>> ExtractFormFieldsAsync(byte[] pdfBytes)
        {
            var formFields = new List<FormField>();

            using (var reader = new PdfReader(pdfBytes))
            {
                var acroFields = reader.AcroFields;
                var fieldNames = acroFields.Fields.Keys;

                foreach (string fieldName in fieldNames)
                {
                    var field = acroFields.Fields[fieldName];
                    var fieldPositions = acroFields.GetFieldPositions(fieldName);

                    if (fieldPositions != null && fieldPositions.Count > 0)
                    {
                        var position = fieldPositions[0];

                        var formField = new FormField
                        {
                            Name = fieldName,
                            Type = DetermineFieldType(acroFields, fieldName),
                            X = position.position.Left,
                            Y = position.position.Bottom,
                            Width = position.position.Width,
                            Height = position.position.Height,
                            PageNumber = position.page,
                            IsRequired = IsFieldRequired(acroFields, fieldName),
                            Options = GetFieldOptions(acroFields, fieldName)
                        };

                        formFields.Add(formField);
                    }
                }
            }

            return formFields;
        }

        private string DetermineFieldType(AcroFields acroFields, string fieldName)
        {
            var fieldType = acroFields.GetFieldType(fieldName);
            return fieldType switch
            {
                AcroFields.FIELD_TYPE_TEXT => "Text",
                AcroFields.FIELD_TYPE_CHECKBOX => "Checkbox",
                AcroFields.FIELD_TYPE_RADIOBUTTON => "RadioButton",
                AcroFields.FIELD_TYPE_LIST => "Dropdown",
                AcroFields.FIELD_TYPE_COMBO => "ComboBox",
                AcroFields.FIELD_TYPE_SIGNATURE => "Signature",
                _ => "Unknown"
            };
        }

        private bool IsFieldRequired(AcroFields acroFields, string fieldName)
        {
            var fieldFlags = acroFields.GetFieldFlags(fieldName);
            return (fieldFlags & PdfFormField.FF_REQUIRED) != 0;
        }

        private string[] GetFieldOptions(AcroFields acroFields, string fieldName)
        {
            var options = acroFields.GetListSelection(fieldName);
            return options ?? new string[0];
        }
    }
}
