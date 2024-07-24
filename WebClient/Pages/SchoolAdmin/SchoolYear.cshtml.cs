using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class SchoolYearModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public SchoolYearModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<SchoolYearData> SchoolYears { get; set; } = new List<SchoolYearData>();
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
                var response = await _httpClient.GetAsync("http://localhost:8001/api/school-years?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var schoolYearResponse = JsonConvert.DeserializeObject<SchoolYearResponse>(jsonResponse);
                    if (schoolYearResponse != null && schoolYearResponse.Success)
                    {
                        SchoolYears = schoolYearResponse.Data ?? new List<SchoolYearData>();
                    }
                    else
                    {
                        ErrorMessage = schoolYearResponse?.Message ?? "Failed to load data.";
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
            return status == "ONGOING" ? "status-active" : "status-inactive";
        }
    }

    public class SchoolYearResponse
    {
        public List<SchoolYearData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class SchoolYearData
    {
        public int SchoolYearId { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
        public string Status { get; set; }
    }
}
