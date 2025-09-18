using Microsoft.Maui.Controls.Shapes;
using PdfFormOverlay.Maui.Services;

namespace PdfFormOverlay.Maui;

public partial class PasswordRecoveryPage : ContentPage
{
    private readonly FormDataService _formDataService;
    private List<string> _securityQuestions;

    public PasswordRecoveryPage()
    {
        InitializeComponent();
        _formDataService = new FormDataService();
        LoadSecurityQuestions();
    }

    private async void LoadSecurityQuestions()
    {
        try
        {
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
            questionsPanel.IsVisible = false;

            _securityQuestions = await _formDataService.GetSecurityQuestionsAsync();

            if (_securityQuestions.Count >= 3)
            {
                question1Label.Text = _securityQuestions[0];
                question2Label.Text = _securityQuestions[1];
                question3Label.Text = _securityQuestions[2];

                questionsPanel.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Error", "Security questions not found. Please reset security settings.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load security questions: {ex.Message}", "OK");
        }
        finally
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
        }
    }

    private async void OnRecoverPasswordClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(answer1Entry.Text) ||
                string.IsNullOrWhiteSpace(answer2Entry.Text) ||
                string.IsNullOrWhiteSpace(answer3Entry.Text))
            {
                await DisplayAlert("Error", "Please answer all security questions.", "OK");
                return;
            }

            recoverButton.IsEnabled = false;
            recoverButton.Text = "Recovering...";

            var answers = new List<string>
            {
                answer1Entry.Text.Trim(),
                answer2Entry.Text.Trim(),
                answer3Entry.Text.Trim()
            };

            var recoveredPassword = await _formDataService.RecoverPasswordAsync(answers);

            if (!string.IsNullOrEmpty(recoveredPassword))
            {
                // Success
                resultFrame.BackgroundColor = Colors.LightGreen;
                resultLabel.Text = "✅ Password Recovery Successful!";
                resultLabel.TextColor = Colors.DarkGreen;

                recoveredPasswordEntry.Text = recoveredPassword;
                recoveredPasswordEntry.IsVisible = true;
                copyPasswordButton.IsVisible = true;

                resultPanel.IsVisible = true;

                // Save the recovered password for immediate use
                SecurityService.SaveUserPassword(recoveredPassword);

                await DisplayAlert("Success", "Your password has been recovered! Please write it down in a safe place.", "OK");
            }
            else
            {
                // Failed
                resultFrame.BackgroundColor = Colors.LightCoral;
                resultLabel.Text = "❌ Recovery Failed";
                resultLabel.TextColor = Colors.DarkRed;

                var incorrectBorder = new Border
                {
                    BackgroundColor = Colors.LightYellow,
                    Padding = 10,
                    StrokeShape = new RoundRectangle { CornerRadius = 5 },
                    Stroke = Colors.Orange,
                    StrokeThickness = 1,
                    Content = new Label
                    {
                        Text = "One or more answers are incorrect. Please check your answers and try again.",
                        FontSize = 12,
                        TextColor = Colors.DarkOrange
                    }
                };

                ((StackLayout)resultFrame.Content).Children.Add(incorrectBorder);
                resultPanel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Recovery failed: {ex.Message}", "OK");
        }
        finally
        {
            recoverButton.IsEnabled = true;
            recoverButton.Text = "Recover Password";
        }
    }

    private async void OnCopyPasswordClicked(object sender, EventArgs e)
    {
        try
        {
            await Clipboard.SetTextAsync(recoveredPasswordEntry.Text);
            await DisplayAlert("Copied", "Password copied to clipboard!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to copy password: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnResetSecurityClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Warning",
            "This will permanently delete all security settings and saved form data. Are you sure?",
            "Yes, Reset Everything", "Cancel");

        if (confirm)
        {
            var finalConfirm = await DisplayAlert("Final Warning",
                "This action cannot be undone. All your saved form data will be lost forever.",
                "I Understand - Delete All", "Cancel");

            if (finalConfirm)
            {
                try
                {
                    // Clear all security and data
                    SecurityService.ClearStoredPassword();

                    // Delete database file
                    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var databasePath = System.IO.Path.Combine(documentsPath, "databases", "soldiers.db");

                    if (File.Exists(databasePath))
                    {
                        File.Delete(databasePath);
                    }

                    await DisplayAlert("Reset Complete", "All security settings and data have been cleared. You can now set up security again.", "OK");

                    // Navigate back to setup
                    await Navigation.PopToRootAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to reset security: {ex.Message}", "OK");
                }
            }
        }
    }
}