using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SuperVisor
{
    public class IndexModel : PageModel
    {
        public string UserName { get; private set; }

        public IActionResult OnGet()
        {

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {

                return RedirectToPage("/Login");
            }


            UserName = HttpContext.Session.GetString("UserName");
            ViewData["UserName"] = UserName;

            return Page();
        }
    }
}
