using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class ClassGroupModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ClassGroupModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ClassGroupData> ClassGroups { get; set; } = new List<ClassGroupData>();
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
                var response = await _httpClient.GetAsync("https://localhost:7291/api/class-groups?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var classGroupResponse = JsonConvert.DeserializeObject<ClassGroupResponse>(jsonResponse);
                    if (classGroupResponse != null && classGroupResponse.Success)
                    {
                        ClassGroups = classGroupResponse.Data ?? new List<ClassGroupData>();
                    }
                    else
                    {
                        ErrorMessage = classGroupResponse?.Message ?? "Failed to load data.";
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

        // Method to get the CSS class based on the status
        public string GetStatusClass(string status)
        {
            return status == "ACTIVE" ? "status-active" : "status-inactive";
        }

    }
    public class ClassGroupResponse
    {
        public List<ClassGroupData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ClassGroupData
    {
        public int ClassGroupId { get; set; }
        public int SchoolId { get; set; }
        public string Hall { get; set; }
        public int Slot { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    }

}
