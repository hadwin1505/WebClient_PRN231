using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebClient.Pages.SuperVisor
{
    public class ViolationDetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViolationDetailsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public ViolationDetail Violation { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:8001/api/violations/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Violation = JsonConvert.DeserializeObject<ViolationDetail>(jsonResponse);
                    if (Violation == null)
                    {
                        ErrorMessage = "Failed to load violation details.";
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
    }

    public class ViolationDetail
    {
        public int ViolationId { get; set; }
        public string StudentName { get; set; }
        public string ViolationName { get; set; }
        public DateTime Date { get; set; }
        public int ClassId { get; set; }
        public string ViolationGroupName { get; set; }
        public string Status { get; set; }
        public string ViolationTypeName { get; set; }
        public string Description { get; set; }
    }

}
