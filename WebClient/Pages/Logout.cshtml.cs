using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("UserName");

            return RedirectToPage("/Login");
        }
    }
}
