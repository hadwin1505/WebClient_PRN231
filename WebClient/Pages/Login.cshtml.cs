using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LoginModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public LoginRequest Login { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _httpClient.PostAsJsonAsync("http://localhost:8001/api/auths/login", Login);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                // Store the token (e.g., in session or local storage)
                HttpContext.Session.SetString("JWToken", result.Token);
                return RedirectToPage("/Index");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                ErrorMessage = error.Message;
                return Page();
            }
        }
    }

    public class LoginRequest
    {
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
    }
}