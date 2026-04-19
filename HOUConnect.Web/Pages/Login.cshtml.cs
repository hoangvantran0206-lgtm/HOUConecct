using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HOUConnect.Business.Services; // Đảm bảo đã using để gọi Service đăng nhập

namespace HOUConnect.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        // --- ĐÂY LÀ PHẦN BẠN CẦN THÊM ---
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var user = _userService.CheckLogin(Email, Password);

            if (user != null)
            {
                // 1. Lưu thông tin vào Session để dùng cho các trang sau
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("UserRole", user.RoleID);
                HttpContext.Session.SetString("UserName", user.FullName);

                // 2. Kiểm tra quyền để chuyển hướng (Redirect)
                if (user.RoleID == 1)
                {
                    // Nếu là Admin -> Vào thẳng trang quản trị
                    return RedirectToPage("/UserManagement");
                }
                else
                {
                    // Nếu là User/Sinh viên -> Về trang chủ
                    return RedirectToPage("/Index");
                }
            }

            // Nếu sai thông tin, hiện thông báo lỗi
            ViewData["ErrorMessage"] = "Email hoặc mật khẩu không chính xác!";
            return Page();
        }
    }
}