using Microsoft.Maui.Layouts;
using PdfFormOverlay.Maui.Models;

namespace PdfFormOverlay.Maui.Services;

public class FormOverlayService
{
    private readonly FormDataService _formDataService;
    private readonly PdfProcessingService _pdfProcessingService;
    private Dictionary<string, View> _overlayControls;
    private Dictionary<string, object> _fieldValues;

    public FormOverlayService()
    {
        _formDataService = new FormDataService();
        _pdfProcessingService = new PdfProcessingService();
        _overlayControls = new Dictionary<string, View>();
        _fieldValues = new Dictionary<string, object>();
    }

    public async Task<AbsoluteLayout> CreateFormOverlayAsync(List<FormField> formFields, AbsoluteLayout pdfContainer)
    {
        var overlay = new AbsoluteLayout
        {
            BackgroundColor = Colors.Transparent
        };

        foreach (var field in formFields)
        {
            var control = CreateControlForField(field);
            if (control != null)
            {
                _overlayControls[field.Name] = control;

                // Position the control over the PDF field
                AbsoluteLayout.SetLayoutBounds(control, new Rect(
                    field.X,
                    field.Y,
                    field.Width,
                    field.Height));
                AbsoluteLayout.SetLayoutFlags(control, AbsoluteLayoutFlags.None);

                overlay.Children.Add(control);
            }
        }

        return overlay;
    }

    private View CreateControlForField(FormField field)
    {
        return field.Type switch
        {
            "Text" => new Entry
            {
                Placeholder = field.Name,
                BackgroundColor = Colors.White.WithAlpha(0.8f),
                TextChanged = (s, e) => _fieldValues[field.Name] = e.NewTextValue
            },
            "Checkbox" => new CheckBox
            {
                BackgroundColor = Colors.White.WithAlpha(0.8f),
                CheckedChanged = (s, e) => _fieldValues[field.Name] = e.Value
            },
            "Dropdown" => new Picker
            {
                ItemsSource = field.Options?.ToList() ?? new List<string>(),
                BackgroundColor = Colors.White.WithAlpha(0.8f),
                SelectedIndexChanged = (s, e) =>
                {
                    var picker = (Picker)s;
                    _fieldValues[field.Name] = picker.SelectedItem?.ToString() ?? "";
                }
            },
            _ => null
        };
    }

    public async Task<bool> SaveFormDataAsync(string formId, string formName)
    {
        try
        {
            await _formDataService.SaveFormDataAsync(formId, formName, _fieldValues);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LoadFormDataAsync(SavedFormData savedData)
    {
        _fieldValues.Clear();

        foreach (var kvp in savedData.FieldValues)
        {
            _fieldValues[kvp.Key] = kvp.Value;

            // Update UI controls with loaded values
            if (_overlayControls.ContainsKey(kvp.Key))
            {
                var control = _overlayControls[kvp.Key];

                switch (control)
                {
                    case Entry entry:
                        entry.Text = kvp.Value?.ToString() ?? "";
                        break;
                    case CheckBox checkbox:
                        checkbox.IsChecked = Convert.ToBoolean(kvp.Value);
                        break;
                    case Picker picker:
                        picker.SelectedItem = kvp.Value?.ToString();
                        break;
                }
            }
        }
    }

    public async Task<byte[]> GenerateFilledPdfAsync(byte[] originalPdf)
    {
        return await _pdfProcessingService.FillPdfFormAsync(originalPdf, _fieldValues);
    }

    public async Task<bool> SavePdfAsync(byte[] filledPdf, string fileName, string path = null)
    {
        return await _pdfProcessingService.SavePdfToLocationAsync(filledPdf, fileName, path);
    }

    public async Task<bool> PrintPdfAsync(byte[] filledPdf)
    {
        return await _pdfProcessingService.PrintPdfAsync(filledPdf);
    }
}