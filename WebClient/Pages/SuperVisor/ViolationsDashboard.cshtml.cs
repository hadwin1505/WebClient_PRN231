using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WebClient.Pages.SuperVisor
{
    public class ViolationsDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationsDashboardModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ViolationData> Violations { get; set; } = new List<ViolationData>();
        public Dictionary<int, string> ClassNames { get; set; } = new Dictionary<int, string>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "You need to log in to view this page.";
                RedirectToPage("/Login");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Fetch violations
                var violationsResponse = await _httpClient.GetAsync("http://localhost:8001/api/violations?sortOrder=id");
                if (violationsResponse.IsSuccessStatusCode)
                {
                    var jsonResponse = await violationsResponse.Content.ReadAsStringAsync();
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
                    ErrorMessage = $"API request failed with status code: {violationsResponse.StatusCode}";
                }

                // Fetch classes
                var classesResponse = await _httpClient.GetAsync("http://localhost:8001/api/classes?sortOrder=asc");
                if (classesResponse.IsSuccessStatusCode)
                {
                    var classesJsonResponse = await classesResponse.Content.ReadAsStringAsync();
                    var classesData = JsonConvert.DeserializeObject<ClassResponse>(classesJsonResponse);
                    if (classesData != null && classesData.Success)
                    {
                        ClassNames = classesData.Data?.ToDictionary(c => c.ClassId, c => c.Name) ?? new Dictionary<int, string>();
                    }
                    else
                    {
                        ErrorMessage = classesData?.Message ?? "Failed to load classes.";
                    }
                }
                else
                {
                    ErrorMessage = $"API request failed with status code: {classesResponse.StatusCode}";
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

    public class ClassResponse
    {
        public List<ClassData> Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ClassData
    {
        public int ClassId { get; set; }
        public string Name { get; set; }
        public int SchoolYearId { get; set; }
        public int ClassGroupId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Code { get; set; }
        public int Grade { get; set; }
        public int TotalPoint { get; set; }
    }
}
