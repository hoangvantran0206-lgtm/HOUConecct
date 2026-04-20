using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HOUConnect.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SongService _songService;
        public IndexModel(SongService songService) => _songService = songService;

        // Danh sách bài hát hiển thị trên Feed
        public List<SongDTO> ListSongs { get; set; } = new();

        public void OnGet()
        {
            // Luôn cho phép vào trang chủ, không kiểm tra Session Redirect ở đây
            ListSongs = _songService.GetAllSongs();
        }
    }
}
