using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

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

                HttpContext.Session.SetString("JWToken", result.Token);

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(result.Token);
                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                var userId = token.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "UserId is missing in the token.";
                    return Page();
                }

                var userResponse = await _httpClient.GetAsync($"http://localhost:8001/api/users/{userId}");
                if (userResponse.IsSuccessStatusCode)
                {

                    var userResponseData = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
                    if (userResponseData != null)
                    {
                        HttpContext.Session.SetString("UserName", userResponseData.Data.UserName);
                    }
                    else
                    {
                        ErrorMessage = "Failed to parse user details.";
                        return Page();
                    }
                }
                else
                {
                    var error = await userResponse.Content.ReadFromJsonAsync<ErrorResponse>();
                    ErrorMessage = $"Error fetching user details: {error.Message}";
                    return Page();
                }

                if (roleClaim == "ADMIN")
                {
                    return RedirectToPage("/Admin/AdminMain");
                }
                else if (roleClaim == "SUPERVISOR")
                {
                    return RedirectToPage("/SuperVisor/Index");
                }
                else if (roleClaim == "TEACHER")
                {
                    return RedirectToPage("/Teacher/TeacherMain");
                }
                else
                {
                    ErrorMessage = "Unknown role.";
                    return Page();
                }
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                ErrorMessage = $"Login failed: {error.Message}";
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

    public class UserResponse
    {
        public UserData Data { get; set; }
    }

    public class UserData
    {
        public int UserId { get; set; }
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Status { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
    }
}
