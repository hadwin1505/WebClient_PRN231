using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class ViolationsConfigsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationsConfigsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ViolationConfig> ViolationConfigs { get; set; } = new List<ViolationConfig>();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page if no token is available
                ErrorMessage = "You need to log in to view this page.";
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("http://localhost:8001/api/violation-configs?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                    if (apiResponse != null && apiResponse.Success)
                    {
                        ViolationConfigs = apiResponse.Data ?? new List<ViolationConfig>();
                    }
                    else
                    {
                        ErrorMessage = apiResponse?.Message ?? "Failed to load data.";
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

            return Page();
        }

        public class ApiResponse
        {
            public List<ViolationConfig> Data { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class ViolationConfig
        {
            public int ViolationConfigId { get; set; }
            public int ViolationTypeId { get; set; }
            public string ViolationTypeName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public int MinusPoints { get; set; }
        }
    }
}
