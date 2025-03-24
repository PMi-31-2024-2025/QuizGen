using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizGen.API.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsAuthenticated { get; private set; }

        public void OnGet()
        {
            IsAuthenticated = Request.Cookies.ContainsKey("AuthToken");
        }
    }
} 