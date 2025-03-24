using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuizGen.API.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
            ErrorMessage = string.Empty;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // If already logged in, redirect to home page
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.LoginAsync(LoginRequest);
                
                if (result.Success)
                {
                    // Store the token and user info in cookies
                    SetAuthCookies(result.Data);
                    
                    return RedirectToPage("/Index");
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        private void SetAuthCookies(AuthResult authResult)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(1)
            };

            Response.Cookies.Append("AuthToken", authResult.AccessToken, cookieOptions);
            Response.Cookies.Append("UserId", authResult.UserId.ToString(), cookieOptions);
            Response.Cookies.Append("UserName", authResult.Username, cookieOptions);
        }
    }
} 