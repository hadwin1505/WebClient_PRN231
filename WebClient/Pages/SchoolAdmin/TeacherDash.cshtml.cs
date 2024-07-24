using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class TeacherDashModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public TeacherDashModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<TeacherData> Teachers { get; set; } = new List<TeacherData>();
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
                var response = await _httpClient.GetAsync("https://localhost:7291/api/teachers?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var teacherResponse = JsonConvert.DeserializeObject<TeacherResponse>(jsonResponse);
                    if (teacherResponse != null && teacherResponse.Success)
                    {
                        Teachers = teacherResponse.Data ?? new List<TeacherData>();
                    }
                    else
                    {
                        ErrorMessage = teacherResponse?.Message ?? "Failed to load data.";
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

    public class TeacherResponse
    {
        public List<TeacherData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class TeacherData
    {
        public int TeacherId { get; set; }
        public string Code { get; set; }
        public string TeacherName { get; set; }
        public string SchoolName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public int RoleId { get; set; }
        public bool Sex { get; set; }
    }
}
