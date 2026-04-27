using HOUConnect.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace HOUConnect.Business.Services
{
    public class SongService
    {
        private readonly SongDAL _songDAL;
        public SongService(SongDAL songDAL) => _songDAL = songDAL;

        public List<GenreDTO> GetGenres()
        {
            var dt = _songDAL.GetGenres();
            var list = new List<GenreDTO>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new GenreDTO
                {
                    GenreID = (int)dr["PK_iGenreID"],
                    GenreName = dr["sGenreName"].ToString()
                });
            }
            return list;
        }

        public string ValidateAndUpload(string name, int genreId, string fileName, int userId)
        {
            if (string.IsNullOrEmpty(name)) return "Tên bài hát không được để trống!";
            if (genreId <= 0) return "Vui lòng chọn thể loại!";

            bool success = _songDAL.InsertSong(name, genreId, fileName, userId);
            return success ? "Success" : "Lỗi lưu dữ liệu!";
        }

        /// <summary>
        /// Lấy danh sách bài hát có hỗ trợ bộ lọc tìm kiếm
        /// </summary>
        /// <param name="search">Từ khóa tìm kiếm (tên bài hát hoặc uploader)</param>
        public List<SongDTO> GetAllSongs(string search = "")
        {
            // Truyền tham số search xuống tầng DAL
            DataTable dt = _songDAL.GetAllSongs(search);
            List<SongDTO> list = new List<SongDTO>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SongDTO
                {
                    SongID = Convert.ToInt32(dr["PK_iSongID"]),
                    SongName = dr["sSongName"].ToString() ?? "",
                    FullName = dr["UploaderName"].ToString() ?? "",
                    FileUrl = "/uploads/" + dr["sFileUrl"].ToString(),
                    CreatedAt = Convert.ToDateTime(dr["dUploadDate"])
                });
            }
            return list;
        }

    }
}