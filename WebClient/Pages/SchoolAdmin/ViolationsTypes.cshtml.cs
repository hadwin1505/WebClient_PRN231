using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebClient.Pages.SchoolAdmin
{
    public class ViolationsTypesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationsTypesModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ViolationTypeData> ViolationTypes { get; set; } = new List<ViolationTypeData>();
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
                var response = await _httpClient.GetAsync("http://localhost:8001/api/violation-types?sortOrder=asc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                    if (apiResponse != null && apiResponse.Success)
                    {
                        ViolationTypes = apiResponse.Data ?? new List<ViolationTypeData>();
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
        }

        public class ApiResponse
        {
            public List<ViolationTypeData> Data { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class ViolationTypeData
        {
            public int ViolationTypeId { get; set; }
            public string VioTypeName { get; set; }
            public int ViolationGroupId { get; set; }
            public string VioGroupName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }
    }
}
