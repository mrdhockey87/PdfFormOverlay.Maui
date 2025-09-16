namespace PdfFormOverlay.Maui;

public partial class LoginPage : ContentPage
{
    private readonly FormDataService _formDataService;

    public LoginPage()
    {
        InitializeComponent();
        _formDataService = new FormDataService();
        CheckSecuritySetup();
    }

    private async void CheckSecuritySetup()
    {
        try
        {
            var isSetup = await _formDataService.IsSecuritySetupAsync();
            setupButton.IsVisible = !isSetup;

            if (!isSetup)
            {
                loginButton.IsEnabled = false;
                passwordEntry.IsEnabled = false;
                await DisplayAlert("Setup Required", "Please set up security before using the application.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to check security setup: {ex.Message}", "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your password.", "OK");
                return;
            }

            loginButton.IsEnabled = false;
            loginButton.Text = "Logging in...";

            var isValid = await _formDataService.ValidateUserPasswordAsync(passwordEntry.Text);

            if (isValid)
            {
                // Save password for session
                SecurityService.SaveUserPassword(passwordEntry.Text);

                // Navigate to main application
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Error", "Invalid password. Please try again.", "OK");
                passwordEntry.Text = "";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            loginButton.IsEnabled = true;
            loginButton.Text = "Login";
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PasswordRecoveryPage());
    }

    private async void OnSetupSecurityClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SecuritySetupPage());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckSecuritySetup();
    }
}