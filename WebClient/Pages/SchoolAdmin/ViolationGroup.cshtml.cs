using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class ViolationGroupModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationGroupModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ViolationGroupData> ViolationGroups { get; set; } = new List<ViolationGroupData>();
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
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("http://localhost:8001/api/violation-groups?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var violationGroupResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                    if (violationGroupResponse != null && violationGroupResponse.Success)
                    {
                        ViolationGroups = violationGroupResponse.Data ?? new List<ViolationGroupData>();
                    }
                    else
                    {
                        ErrorMessage = violationGroupResponse?.Message ?? "Failed to load data.";
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

        public class ApiResponse
        {
            public List<ViolationGroupData> Data { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class ViolationGroupData
        {
            public int ViolationGroupId { get; set; }
            public int SchoolId { get; set; }
            public string SchoolName { get; set; }
            public string VioGroupCode { get; set; }
            public string VioGroupName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }
    }
}
