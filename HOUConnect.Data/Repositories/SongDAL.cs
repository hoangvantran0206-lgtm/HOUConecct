using HOUConnect.Data;
using Microsoft.Data.SqlClient;
using System.Data;

public class SongDAL
{
    private readonly SqlHelper _sqlHelper;
    public SongDAL(SqlHelper sqlHelper) => _sqlHelper = sqlHelper;

    // 1. Lấy danh sách thể loại để đổ vào Dropdown
    public DataTable GetGenres()
    {
        return _sqlHelper.ExecuteQuery("sp_GetAllGenres", null);
    }

    // 2. Lưu bài hát mới
    public bool InsertSong(string name, int genreId, string fileUrl, int userId)
    {
        SqlParameter[] paras = {
            new SqlParameter("@sSongName", name),
            new SqlParameter("@iGenreID", genreId),
            new SqlParameter("@sFileUrl", fileUrl),
            new SqlParameter("@iUserID", userId)
        };
        return _sqlHelper.ExecuteNonQuery("sp_InsertSong", paras) > 0;
    }
    public virtual DataTable GetAllSongs(string search = "")
    {
        // Nếu search null thì gán rỗng, SQL sẽ lấy hết. Nếu có chữ "em", nó sẽ lọc.
        search = search ?? "";

        SqlParameter[] paras = new SqlParameter[]
        {
        // Tên này phải GIỐNG HỆT tên trong Procedure (đã xác định là @sSearchTerm)
        new SqlParameter("@sSearchTerm", search)
        };

        // QUAN TRỌNG: Sếp nhìn kỹ xem có chữ ", paras" ở cuối không? 
        // Thiếu cái này là SQL nó không nhận được chữ sếp gõ đâu!
        return _sqlHelper.ExecuteQuery("sp_GetAllSongs", paras);
    }
}