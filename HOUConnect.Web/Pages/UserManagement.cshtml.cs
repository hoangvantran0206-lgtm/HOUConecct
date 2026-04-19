using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HOUConnect.Web.Pages
{
    public class UserManagementModel : PageModel
    {
        private readonly UserService _userService;
        public UserManagementModel(UserService userService) => _userService = userService;

        [BindProperty(SupportsGet = true)]
        public string? CurrentSearch { get; set; }

        public List<UserDTO> Users { get; set; } = new();

        // Load danh sách khi mở trang hoặc tìm kiếm
        public void OnGet()
        {
            Users = _userService.GetUsers(CurrentSearch);
        }

        // Xử lý khi nhấn nút Khóa/Mở khóa
        public IActionResult OnPostToggleStatus(int id)
        {
            _userService.ToggleUserLock(id);

            // Dùng Redirect để tránh lỗi NullReferenceException khi quay lại View
            return RedirectToPage(new { search = CurrentSearch });
        }
    }
}
