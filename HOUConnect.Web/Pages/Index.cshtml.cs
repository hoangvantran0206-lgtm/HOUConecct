using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly SongService _songService;

    // Sếp đổi tên từ 'Songs' thành 'ListSongs' cho giống file .cshtml nhé
    public List<SongDTO> ListSongs { get; set; } = new List<SongDTO>();

    [BindProperty(Name = "searchString", SupportsGet = true)] // Thêm Name="..." cho chắc cú
    public string? SearchString { get; set; }

    public IndexModel(SongService songService)
    {
        _songService = songService;
    }

    public void OnGet()
    {
        ViewData["CurrentFilter"] = SearchString;

        // Đổ dữ liệu vào ĐÚNG cái biến mà file .cshtml đang hiển thị
        ListSongs = _songService.GetAllSongs(SearchString ?? "");
    }
}