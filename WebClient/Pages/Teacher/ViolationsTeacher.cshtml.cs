using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.Teacher
{
    public class ViolationsTeacherModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationsTeacherModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ViolationData> Violations { get; set; } = new List<ViolationData>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page if no token is available
                ErrorMessage = "You need to log in to view this page.";
                // Redirect to the login page or display an error message
                RedirectToPage("/Login");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7291/api/violations?sortOrder=id");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var violationResponse = JsonConvert.DeserializeObject<ViolationResponse>(jsonResponse);
                    if (violationResponse != null && violationResponse.Success)
                    {
                        Violations = violationResponse.Data ?? new List<ViolationData>();
                    }
                    else
                    {
                        ErrorMessage = violationResponse?.Message ?? "Failed to load data.";
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

    public class ViolationResponse
    {
        public List<ViolationData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ViolationData
    {
        public int ViolationId { get; set; }
        public int ClassId { get; set; }
        public int StudentInClassId { get; set; }
        public string StudentName { get; set; }
        public int ViolationTypeId { get; set; }
        public string ViolationTypeName { get; set; }
        public int ViolationGroupId { get; set; }
        public string ViolationGroupName { get; set; }
        public int TeacherId { get; set; }
        public string ViolationName { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public string Status { get; set; }
    }
}