using HOUConnect.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
