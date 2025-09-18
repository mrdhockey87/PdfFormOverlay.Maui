using PdfFormOverlay.Maui.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // PDF Form Analyzer converted to PDFsharp
    public class PdfFormAnalyzer
    {
        public static async Task<List<FormField>> ExtractFormFieldsAsync(byte[] pdfBytes)
        {
            var formFields = new List<FormField>();

            using (var stream = new MemoryStream(pdfBytes))
            {
                var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

                if (document.AcroForm != null)
                {
                    // Iterate through the field names and get the actual fields
                    foreach (string fieldName in document.AcroForm.Fields.Names)
                    {
                        try
                        {
                            var field = document.AcroForm.Fields[fieldName];
                            if (field is PdfAcroField acroField)
                            {
                                var formField = new FormField
                                {
                                    Name = acroField.Name,
                                    Type = PdfFormAnalyzer.DetermineFieldType(acroField),
                                    X = (float)PdfFormAnalyzer.GetFieldX(acroField),
                                    Y = (float)PdfFormAnalyzer.GetFieldY(acroField),
                                    Width = (float)PdfFormAnalyzer.GetFieldWidth(acroField),
                                    Height = (float)PdfFormAnalyzer.GetFieldHeight(acroField),
                                    PageNumber = PdfFormAnalyzer.GetFieldPageNumber(acroField, document),
                                    IsRequired = PdfFormAnalyzer.IsFieldRequired(acroField),
                                    Options = PdfFormAnalyzer.GetFieldOptions(acroField)
                                };

                                formFields.Add(formField);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing field {fieldName}: {ex.Message}");
                        }
                    }
                }

                document.Close();
            }

            return formFields;
        }

        private static string DetermineFieldType(PdfAcroField field)
        {
            return field switch
            {
                PdfTextField => "Text",
                PdfCheckBoxField => "Checkbox",
                PdfRadioButtonField => "RadioButton",
                PdfListBoxField => "Dropdown",
                PdfComboBoxField => "ComboBox",
                PdfSignatureField => "Signature",
                _ => "Unknown"
            };
        }

        private static double GetFieldX(PdfAcroField field)
        {
            try
            {
                // PDFsharp handles coordinates differently than iTextSharp
                // This is a simplified approach - you may need to adjust based on your needs
                var rect = field.Elements.GetRectangle("/Rect");
                return rect?.X1 ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetFieldY(PdfAcroField field)
        {
            try
            {
                var rect = field.Elements.GetRectangle("/Rect");
                return rect?.Y1 ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetFieldWidth(PdfAcroField field)
        {
            try
            {
                var rect = field.Elements.GetRectangle("/Rect");
                return rect != null ? Math.Abs(rect.X2 - rect.X1) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetFieldHeight(PdfAcroField field)
        {
            try
            {
                var rect = field.Elements.GetRectangle("/Rect");
                return rect != null ? Math.Abs(rect.Y2 - rect.Y1) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private static int GetFieldPageNumber(PdfAcroField field, PdfDocument document)
        {
            try
            {
                // Simplified approach: Check if the field has a page reference
                // In PDFsharp, fields can have a page reference in their elements
                var pageRef = field.Elements.GetReference("/P");
                if (pageRef != null)
                {
                    // Find the page index
                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        if (document.Pages[i].Reference == pageRef)
                        {
                            return i + 1; // 1-based page numbering
                        }
                    }
                }

                // Alternative approach: Check all pages for annotations
                for (int i = 0; i < document.Pages.Count; i++)
                {
                    var page = document.Pages[i];
                    if (page.Annotations != null)
                    {
                        foreach (var annotationItem in page.Annotations)
                        {
                            // Cast to PdfAnnotation to access Elements
                            if (annotationItem is PdfAnnotation annotation)
                            {
                                var fieldName = annotation.Elements.GetString("/T");
                                if (fieldName == field.Name)
                                {
                                    return i + 1; // 1-based page numbering
                                }
                            }
                            // Alternative: check if it's a PdfDictionary
                            else if (annotationItem is PdfDictionary dict)
                            {
                                var fieldName = dict.Elements.GetString("/T");
                                if (fieldName == field.Name)
                                {
                                    return i + 1; // 1-based page numbering
                                }
                            }
                        }
                    }
                }

                return 1; // Default to first page
            }
            catch
            {
                return 1;
            }
        }

        private static bool IsFieldRequired(PdfAcroField field)
        {
            try
            {
                var flags = field.Elements.GetInteger("/Ff");
                return (flags & 2) != 0; // Required flag
            }
            catch
            {
                return false;
            }
        }

        private static string[] GetFieldOptions(PdfAcroField field)
        {
            try
            {
                // For PDFsharp, we need to access the field options differently
                // This is a simplified approach - actual implementation may vary
                var optionsArray = field.Elements.GetArray("/Opt");
                if (optionsArray != null)
                {
                    var options = new List<string>();
                    foreach (var item in optionsArray)
                    {
                        if (item is PdfString pdfString)
                        {
                            options.Add(pdfString.Value);
                        }
                        else if (item is PdfArray pdfArray && pdfArray.Elements.Count > 0)
                        {
                            // Sometimes options are stored as [value, display] pairs
                            if (pdfArray.Elements[0] is PdfString displayString)
                            {
                                options.Add(displayString.Value);
                            }
                        }
                    }
                    return [.. options];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }
    }
}