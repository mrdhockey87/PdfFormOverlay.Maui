using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // PDF Processing Service converted to PDFsharp
    public class PdfProcessingService
    {
        public static async Task<byte[]> FillPdfFormAsync(byte[] originalPdf, Dictionary<string, object> fieldValues)
        {
            using var inputStream = new MemoryStream(originalPdf);
            using var outputStream = new MemoryStream();
            // Open the PDF document
            var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

            // Get the AcroForm
            if (document.AcroForm != null)
            {
                foreach (var kvp in fieldValues)
                {
                    var fieldName = kvp.Key;
                    var fieldValue = kvp.Value?.ToString() ?? "";

                    try
                    {
                        // Find the field in the AcroForm
                        var field = document.AcroForm.Fields[fieldName];
                        if (field != null)
                        {
                            if (field is PdfTextField textField)
                            {
                                textField.Text = fieldValue;
                            }
                            else if (field is PdfCheckBoxField checkBox)
                            {
                                checkBox.Checked = bool.TryParse(fieldValue, out bool isChecked) && isChecked;
                            }
                            else if (field is PdfComboBoxField comboBox)
                            {
                                // For combo boxes, set the value directly
                                comboBox.Value = new PdfString(fieldValue);
                            }
                            else if (field is PdfListBoxField listBox)
                            {
                                // For list boxes, set the value directly
                                listBox.Value = new PdfString(fieldValue);
                            }
                            else if (field is PdfRadioButtonField radioButton)
                            {
                                // For radio buttons, set the value
                                radioButton.Value = new PdfName(fieldValue);
                            }
                            else
                            {
                                // Generic field handling - try to set the value
                                try
                                {
                                    field.Value = new PdfString(fieldValue);
                                }
                                catch
                                {
                                    // If direct value setting fails, try setting as text
                                    field.Elements.SetString("/V", fieldValue);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting field {fieldName}: {ex.Message}");
                    }
                }

                // Flatten the form (make fields non-editable)
                // Note: PDFsharp doesn't have direct form flattening like iTextSharp
                // You may need to implement custom flattening if required
            }

            // Save to output stream
            document.Save(outputStream);
            document.Close();

            return outputStream.ToArray();
        }

        public static async Task<bool> SavePdfToLocationAsync(byte[] pdfBytes, string fileName, string targetPath = null)
        {
            try
            {
                string filePath;

                if (string.IsNullOrEmpty(targetPath))
                {
                    filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                }
                else
                {
                    filePath = Path.Combine(targetPath, fileName);
                }

                await File.WriteAllBytesAsync(filePath, pdfBytes);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving PDF: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> PrintPdfAsync(byte[] pdfBytes)
        {
            try
            {
                // Use standard .NET file operations to avoid WinRT issues
                var tempPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                // Use platform-specific file launching
#if WINDOWS
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"file:///{tempPath}"));
#else
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(tempPath),
                    Title = "Print PDF"
                });
#endif
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error printing PDF: {ex.Message}");
                return false;
            }
        }
    }
}