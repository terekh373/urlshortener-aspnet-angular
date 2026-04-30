using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UrlShortener.API.Pages;

public class RegisterModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        using var http = new HttpClient();
        var response = await http.PostAsJsonAsync(
            $"{Request.Scheme}://{Request.Host}/api/auth/register",
            new { username = Username, email = Email, password = Password }
        );

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Registration failed. Username may already be taken.";
            return Page();
        }

        SuccessMessage = "Account created! You can now login.";
        return Page();
    }
}