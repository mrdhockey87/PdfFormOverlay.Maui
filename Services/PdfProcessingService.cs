using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // PDF Processing Service (unchanged from previous version)
    public class PdfProcessingService
    {
        public async Task<byte[]> FillPdfFormAsync(byte[] originalPdf, Dictionary<string, object> fieldValues)
        {
            using (var output = new MemoryStream())
            {
                using (var reader = new PdfReader(originalPdf))
                {
                    using (var stamper = new PdfStamper(reader, output))
                    {
                        var acroFields = stamper.AcroFields;

                        foreach (var kvp in fieldValues)
                        {
                            var fieldName = kvp.Key;
                            var fieldValue = kvp.Value?.ToString() ?? "";

                            try
                            {
                                acroFields.SetField(fieldName, fieldValue);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error setting field {fieldName}: {ex.Message}");
                            }
                        }

                        stamper.FormFlattening = true;
                    }
                }

                return output.ToArray();
            }
        }

        public async Task<bool> SavePdfToLocationAsync(byte[] pdfBytes, string fileName, string targetPath = null)
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

        public async Task<bool> PrintPdfAsync(byte[] pdfBytes)
        {
            try
            {
                var tempPath = Path.GetTempFileName() + ".pdf";
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(tempPath),
                    Title = "Print PDF"
                });

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
