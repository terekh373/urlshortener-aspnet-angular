using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UrlShortener.API.Pages;

public class AboutModel : PageModel
{
    // For simplicity, we store the description in a static variable. In a real app, this would come from a database or config file.
    private static string _description =
        "Our URL Shortener uses a random 6-character code generator. " +
        "The algorithm picks 6 random characters from a set of 62 symbols " +
        "(a-z, A-Z, 0-9), giving over 56 billion unique combinations. " +
        "Each code is checked for uniqueness before saving to the database.";

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
    public bool IsEditing { get; set; }

    public void OnGet()
    {
        Description = _description;
        IsAdmin = User.IsInRole("Admin");
    }

    public IActionResult OnPost()
    {
        if (!User.IsInRole("Admin"))
            return Forbid();

        _description = Description;
        return RedirectToPage();
    }
}