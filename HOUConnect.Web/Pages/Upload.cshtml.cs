using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HOUConnect.Web.Pages
{
    public class UploadModel : PageModel
    {
        private readonly SongService _songService;
        private readonly IWebHostEnvironment _env;

        public UploadModel(SongService songService, IWebHostEnvironment env)
        {
            _songService = songService;
            _env = env;
        }

        [BindProperty] public string SongName { get; set; }
        [BindProperty] public int SelectedGenreID { get; set; }
        [BindProperty] public IFormFile MusicFile { get; set; }
        public List<GenreDTO> Genres { get; set; }

        public void OnGet()
        {
            Genres = _songService.GetGenres(); // Lấy danh sách thể loại khi vừa load trang
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (MusicFile != null)
            {
                // Tư duy bảo mật: Đổi tên file để tránh trùng và mã độc
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + MusicFile.FileName;
                string path = Path.Combine(_env.WebRootPath, "uploads", uniqueFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await MusicFile.CopyToAsync(stream);
                }

                // Gọi Business (UserID tạm thời lấy từ Session hoặc mặc định 1)
                _songService.ValidateAndUpload(SongName, SelectedGenreID, uniqueFileName, 1);
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
