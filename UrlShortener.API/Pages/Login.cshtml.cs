using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UrlShortener.API.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        using var http = new HttpClient();
        var response = await http.PostAsJsonAsync(
            $"{Request.Scheme}://{Request.Host}/api/auth/login",
            new { username = Username, password = Password }
        );

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Invalid username or password";
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();

        TempData["Token"] = result?.Token;
        TempData["Username"] = result?.Username;
        TempData["Role"] = result?.Role;

        return RedirectToPage("/Index");
    }
}

public record LoginResult(string Token, string Username, string Role);