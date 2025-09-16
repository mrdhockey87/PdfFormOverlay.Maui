using PdfFormOverlay.Maui.Models;
using PdfFormOverlay.Maui.Services;

namespace PdfFormOverlay.Maui;

public partial class PdfFormPage : ContentPage
{
    private readonly FormOverlayService _overlayService;
    private readonly PdfFormAnalyzer _formAnalyzer;
    private readonly FormDataService _formDataService;
    private byte[] _originalPdfBytes;
    private string _currentFormId;
    private string _currentFormName;
    private List<FormField> _formFields;
    private bool _isSessionLocked = false;

    public PdfFormPage()
    {
        InitializeComponent();
        _overlayService = new FormOverlayService();
        _formAnalyzer = new PdfFormAnalyzer();
        _formDataService = new FormDataService();

        // Show security status
        securityStatusBar.IsVisible = SecurityService.IsPasswordSet();
        securityStatusLabel.Text = $"Secure Session - User authenticated";
    }

    public async Task LoadPdfFormAsync(byte[] pdfBytes, string formName)
    {
        if (_isSessionLocked)
        {
            await DisplayAlert("Session Locked", "Please unlock the session first.", "OK");
            return;
        }

        _originalPdfBytes = pdfBytes;
        _currentFormName = formName;
        _currentFormId = GenerateFormId(pdfBytes);

        // Analyze PDF for form fields
        _formFields = await _formAnalyzer.ExtractFormFieldsAsync(pdfBytes);

        // Load PDF in viewer
        pdfView.LoadPdf(pdfBytes);

        // Create form overlay
        if (_formFields.Any())
        {
            var overlay = await _overlayService.CreateFormOverlayAsync(_formFields, pdfContainer);
            pdfContainer.Children.Add(overlay);

            // Show form controls
            formActionsPanel.IsVisible = true;
        }
    }

    private string GenerateFormId(byte[] pdfBytes)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha256.ComputeHash(pdfBytes);
            return Convert.ToHexString(hash)[..16];
        }
    }

    private async void OnLockClicked(object sender, EventArgs e)
    {
        LockSession();
    }

    private void LockSession()
    {
        _isSessionLocked = true;
        securityOverlay.IsVisible = true;
        unlockPasswordEntry.Text = "";
    }

    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(unlockPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your password.", "OK");
                return;
            }

            var isValid = await _formDataService.ValidateUserPasswordAsync(unlockPasswordEntry.Text);

            if (isValid)
            {
                _isSessionLocked = false;
                securityOverlay.IsVisible = false;
                unlockPasswordEntry.Text = "";
            }
            else
            {
                await DisplayAlert("Error", "Invalid password.", "OK");
                unlockPasswordEntry.Text = "";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unlock failed: {ex.Message}", "OK");
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PasswordRecoveryPage());
    }

    private async void OnSaveDataClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            saveDataButton.IsEnabled = false;
            saveDataButton.Text = "Saving...";

            var success = await _overlayService.SaveFormDataAsync(_currentFormId, _currentFormName);

            if (success)
            {
                await DisplayAlert("Success", "Form data saved successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save form data.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Save failed: {ex.Message}", "OK");
        }
        finally
        {
            saveDataButton.IsEnabled = true;
            saveDataButton.Text = "Save Data";
        }
    }

    private async void OnLoadDataClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            loadDataButton.IsEnabled = false;
            loadDataButton.Text = "Loading...";

            var savedForms = await _formDataService.GetSavedFormsAsync(_currentFormId);

            if (savedForms.Any())
            {
                var formNames = savedForms.Select(f => $"{f.FormName} - {f.SavedDate:yyyy-MM-dd HH:mm}").ToArray();
                var selectedForm = await DisplayActionSheet("Load Saved Data", "Cancel", null, formNames);

                if (selectedForm != "Cancel" && selectedForm != null)
                {
                    var selectedIndex = Array.IndexOf(formNames, selectedForm);
                    await _overlayService.LoadFormDataAsync(savedForms[selectedIndex]);
                    await DisplayAlert("Success", "Form data loaded successfully!", "OK");
                }
            }
            else
            {
                await DisplayAlert("No Data", "No saved data found for this form.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Load failed: {ex.Message}", "OK");
        }
        finally
        {
            loadDataButton.IsEnabled = true;
            loadDataButton.Text = "Load Data";
        }
    }

    private async void OnDeleteDataClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            var savedForms = await _formDataService.GetSavedFormsAsync(_currentFormId);

            if (savedForms.Any())
            {
                var formNames = savedForms.Select(f => $"{f.FormName} - {f.SavedDate:yyyy-MM-dd HH:mm}").ToArray();
                var selectedForm = await DisplayActionSheet("Delete Saved Data", "Cancel", null, formNames);

                if (selectedForm != "Cancel" && selectedForm != null)
                {
                    var confirm = await DisplayAlert("Confirm Delete",
                        "Are you sure you want to delete this saved form data?", "Delete", "Cancel");

                    if (confirm)
                    {
                        var selectedIndex = Array.IndexOf(formNames, selectedForm);
                        var formToDelete = savedForms[selectedIndex];

                        var success = await _formDataService.DeleteSavedFormAsync(formToDelete.FormId, formToDelete.SavedDate);

                        if (success)
                        {
                            await DisplayAlert("Success", "Form data deleted successfully!", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Error", "Failed to delete form data.", "OK");
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("No Data", "No saved data found for this form.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Delete failed: {ex.Message}", "OK");
        }
    }

    private async void OnSavePdfClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            savePdfButton.IsEnabled = false;
            savePdfButton.Text = "Saving...";

            var filledPdf = await _overlayService.GenerateFilledPdfAsync(_originalPdfBytes);
            var fileName = $"{_currentFormName}_filled_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            var useDefaultLocation = await DisplayAlert("Save Location",
                "Save to Documents folder?", "Yes", "Choose Location");

            string targetPath = null;
            if (!useDefaultLocation)
            {
                targetPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            var success = await _overlayService.SavePdfAsync(filledPdf, fileName, targetPath);

            if (success)
            {
                await DisplayAlert("Success", $"PDF saved as {fileName}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save PDF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Save PDF failed: {ex.Message}", "OK");
        }
        finally
        {
            savePdfButton.IsEnabled = true;
            savePdfButton.Text = "Save PDF";
        }
    }

    private async void OnPrintPdfClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            printPdfButton.IsEnabled = false;
            printPdfButton.Text = "Printing...";

            var filledPdf = await _overlayService.GenerateFilledPdfAsync(_originalPdfBytes);
            var success = await _overlayService.PrintPdfAsync(filledPdf);

            if (!success)
            {
                await DisplayAlert("Error", "Failed to print PDF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Print failed: {ex.Message}", "OK");
        }
        finally
        {
            printPdfButton.IsEnabled = true;
            printPdfButton.Text = "Print PDF";
        }
    }

    private async void OnEmailPdfClicked(object sender, EventArgs e)
    {
        if (_isSessionLocked) { LockSession(); return; }

        try
        {
            emailPdfButton.IsEnabled = false;
            emailPdfButton.Text = "Preparing...";

            var filledPdf = await _overlayService.GenerateFilledPdfAsync(_originalPdfBytes);
            var fileName = $"{_currentFormName}_filled_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // Save to temp location for email
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllBytesAsync(tempPath, filledPdf);

            var message = new EmailMessage
            {
                Subject = $"Completed Form: {_currentFormName}",
                Body = "Please find the completed form attached.",
                Attachments = { new EmailAttachment(tempPath) }
            };

            await Email.ComposeAsync(message);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Email failed: {ex.Message}", "OK");
        }
        finally
        {
            emailPdfButton.IsEnabled = true;
            emailPdfButton.Text = "Email PDF";
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Check if user is still authenticated
        if (!SecurityService.IsPasswordSet())
        {
            Navigation.PushAsync(new LoginPage());
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (_isSessionLocked)
        {
            return true; // Prevent navigation when locked
        }

        return base.OnBackButtonPressed();
    }
}