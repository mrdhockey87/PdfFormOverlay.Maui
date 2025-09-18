using PdfFormOverlay.Maui.Models;
using PdfFormOverlay.Maui.Services;

namespace PdfFormOverlay.Maui;

public partial class SecuritySetupPage : ContentPage
{
    private readonly FormDataService _formDataService;
    private List<string> _predefinedQuestions;

    public SecuritySetupPage()
    {
        InitializeComponent();
        _formDataService = new FormDataService();
        InitializePredefinedQuestions();
    }

    private void InitializePredefinedQuestions()
    {
        _predefinedQuestions = new List<string>
        {
            "What was the name of your first pet?",
            "What city were you born in?",
            "What was your mother's maiden name?",
            "What was the name of your first school?",
            "What was your childhood nickname?",
            "What is your favorite movie?",
            "What was the make of your first car?",
            "What street did you grow up on?",
            "What was your favorite teacher's name?",
            "What is your favorite book?"
        };

        question1Picker.ItemsSource = _predefinedQuestions;
        question2Picker.ItemsSource = _predefinedQuestions;
        question3Picker.ItemsSource = _predefinedQuestions;
    }

    private async void OnSetupSecurityClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(passwordEntry.Text) || passwordEntry.Text.Length < 8)
            {
                await DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                return;
            }

            if (passwordEntry.Text != confirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            if (question1Picker.SelectedIndex == -1 || question2Picker.SelectedIndex == -1 || question3Picker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please select all three security questions.", "OK");
                return;
            }

            if (question1Picker.SelectedIndex == question2Picker.SelectedIndex ||
                question1Picker.SelectedIndex == question3Picker.SelectedIndex ||
                question2Picker.SelectedIndex == question3Picker.SelectedIndex)
            {
                await DisplayAlert("Error", "Please select different security questions.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(answer1Entry.Text) ||
                string.IsNullOrWhiteSpace(answer2Entry.Text) ||
                string.IsNullOrWhiteSpace(answer3Entry.Text))
            {
                await DisplayAlert("Error", "Please provide answers for all security questions.", "OK");
                return;
            }

            // Create security questions list
            var securityQuestions = new List<SecurityQuestion>
            {
                new SecurityQuestion { Question = _predefinedQuestions[question1Picker.SelectedIndex], Answer = answer1Entry.Text },
                new SecurityQuestion { Question = _predefinedQuestions[question2Picker.SelectedIndex], Answer = answer2Entry.Text },
                new SecurityQuestion { Question = _predefinedQuestions[question3Picker.SelectedIndex], Answer = answer3Entry.Text }
            };

            setupButton.IsEnabled = false;
            setupButton.Text = "Setting up...";

            // Setup security
            var success = await _formDataService.SetupUserSecurityAsync(passwordEntry.Text, securityQuestions);

            if (success)
            {
                await DisplayAlert("Success", "Security has been set up successfully!", "OK");
                await Navigation.PopAsync(); // Return to previous page
            }
            else
            {
                await DisplayAlert("Error", "Failed to setup security. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            setupButton.IsEnabled = true;
            setupButton.Text = "Setup Security";
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}