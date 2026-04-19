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
}