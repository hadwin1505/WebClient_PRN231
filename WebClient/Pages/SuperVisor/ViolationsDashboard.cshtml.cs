using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace WebClient.Pages.SuperVisor
{
    public class ViolationsDashboardModel : PageModel
    {
        public List<Violation> Violations { get; set; }

        public void OnGet()
        {
            // Fetch violations from database or any data source
            Violations = new List<Violation>
            {
                new Violation { Id = 1, Description = "Late submission", Date = DateTime.Now.AddDays(-1) },
                new Violation { Id = 2, Description = "Missed meeting", Date = DateTime.Now.AddDays(-2) }
            };
        }

        public class Violation
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
