using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class SuperVisorModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public SuperVisorModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<SupervisorData> Supervisors { get; set; } = new List<SupervisorData>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page if no token is available
                ErrorMessage = "You need to log in to view this page.";
                RedirectToPage("/Login");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7291/api/student-supervisors?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var supervisorResponse = JsonConvert.DeserializeObject<SupervisorResponse>(jsonResponse);
                    if (supervisorResponse != null && supervisorResponse.Success)
                    {
                        Supervisors = supervisorResponse.Data ?? new List<SupervisorData>();
                    }
                    else
                    {
                        ErrorMessage = supervisorResponse?.Message ?? "Failed to load data.";
                    }
                }
                else
                {
                    ErrorMessage = $"API request failed with status code: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Request error: {ex.Message}";
            }
            catch (JsonException ex)
            {
                ErrorMessage = $"JSON parsing error: {ex.Message}";
            }
        }
    }

    public class SupervisorResponse
    {
        public List<SupervisorData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class SupervisorData
    {
        public int StudentSupervisorId { get; set; }
        public int StudentInClassId { get; set; }
        public bool IsSupervisor { get; set; }
        public int SchoolId { get; set; }
        public string Code { get; set; }
        public string SupervisorName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int RoleId { get; set; }
    }
}
