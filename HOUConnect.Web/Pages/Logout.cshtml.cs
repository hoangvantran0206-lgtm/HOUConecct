using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HOUConnect.Web.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // 1. Xóa toàn bộ dữ liệu trong Session (Tên, Role, ID...)
            HttpContext.Session.Clear();

            // 2. Để chắc chắn hơn, có thể xóa từng key cụ thể
            // HttpContext.Session.Remove("UserName");

            // 3. Chuyển hướng người dùng về trang chủ sau khi thoát
            return RedirectToPage("/Index");
        }
    }
}
