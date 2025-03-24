using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Primitives;

namespace QuizGen.API.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordModel(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            StatusMessage = string.Empty;
            ConfirmPassword = string.Empty;
        }

        [BindProperty]
        public ChangePasswordRequest PasswordRequest { get; set; } = new ChangePasswordRequest();

        // Property for binding the nested value directly for model validation
        [BindProperty]
        public string NewPassword 
        { 
            get => PasswordRequest.NewPassword; 
            set => PasswordRequest.NewPassword = value; 
        }

        [BindProperty]
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            var authToken = Request.Cookies["AuthToken"];
            var userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if user is authenticated
            var authToken = Request.Cookies["AuthToken"];
            var userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            if (!int.TryParse(userId, out int currentUserId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Set authentication header for the request
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Request.Headers["Authorization"] = 
                        new StringValues($"Bearer {authToken}");
                }

                // Change password
                var result = await _authService.ChangePasswordAsync(
                    currentUserId,
                    PasswordRequest.CurrentPassword,
                    PasswordRequest.NewPassword);
                
                if (result.Success)
                {
                    StatusMessage = "Your password has been changed successfully.";
                    IsSuccess = true;
                    // Clear the form
                    PasswordRequest = new ChangePasswordRequest();
                    ConfirmPassword = string.Empty;
                    return Page();
                }
                else
                {
                    StatusMessage = result.Message;
                    IsSuccess = false;
                    return Page();
                }
            }
            catch (Exception)
            {
                StatusMessage = "An error occurred while changing your password.";
                IsSuccess = false;
                return Page();
            }
        }
    }
} 