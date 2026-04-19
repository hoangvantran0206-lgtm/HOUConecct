using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HOUConnect.Business.Services;

namespace HOUConnect.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;

        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string FullName { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            string result = _userService.CreateAccount(FullName, Email, Password);

            if (result == "Success")
            {
                return RedirectToPage("Login"); // Chuyển hướng sang trang đăng nhập
            }

            ViewData["ErrorMessage"] = result;
            return Page();
        }
    }
}