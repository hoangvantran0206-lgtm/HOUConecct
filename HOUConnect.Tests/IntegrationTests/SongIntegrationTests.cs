using Xunit;
using HOUConnect.Data.Repositories;
using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using HOUConnect.Data;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace HOUConnect.Tests.IntegrationTests
{
    /// <summary>
    /// Kiểm thử tích hợp module Âm nhạc
    /// Kiểm tra liên kết giữa tbl_Songs, tbl_Genres và tbl_Users
    /// </summary>
    public class SongIntegrationTests
    {
        private readonly SongDAL _songDAL;
        private readonly SongService _songService;
        private readonly SqlHelper _sqlHelper;

        public SongIntegrationTests()
        {
            _sqlHelper = new SqlHelper(TestConfig.ConnectionString);
            _songDAL = new SongDAL(_sqlHelper);
            _songService = new SongService(_songDAL);
        }

        #region Kiểm tra Thể loại (Genre)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_08_GetGenres_ReturnsSeededData()
        {
            // Act: Lấy danh sách thể loại từ DB (Dữ liệu đã Seed: Pop, Ballad, Podcast)
            List<GenreDTO> genres = _songService.GetGenres();

            // Assert: Kiểm tra xem có đủ dữ liệu mẫu đã nạp ban đầu không
            Assert.NotEmpty(genres);
            Assert.Contains(genres, g => g.GenreName == "Pop");
            Assert.Contains(genres, g => g.GenreName == "Ballad");
        }

        #endregion

        #region Kiểm tra Tải bài hát (Upload)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_09_ValidateAndUpload_ValidData_SavesToDB()
        {
            // 1. Arrange: Chuẩn bị thông tin bài hát (UserID=1 là Admin đã Seed)
            string songName = "HOU Integration Song";
            CleanUpSong(songName);

            // 2. Act: Gọi Service thực hiện nghiệp vụ lưu
            string result = _songService.ValidateAndUpload(songName, 1, "test_file.mp3", 1);

            // 3. Assert: Kiểm tra phản hồi và dữ liệu thực tế
            Assert.Equal("Success", result);

            var allSongs = _songService.GetAllSongs(songName);
            Assert.Single(allSongs);
            Assert.Equal(songName, allSongs[0].SongName);

            CleanUpSong(songName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_10_ValidateAndUpload_EmptyName_ReturnsError()
        {
            // Act: Thử lưu bài hát không có tên
            var result = _songService.ValidateAndUpload("", 1, "test.mp3", 1);

            // Assert: Service phải chặn lại trước khi gọi xuống SQL
            Assert.Equal("Tên bài hát không được để trống!", result);
        }

        #endregion

        #region Kiểm tra Truy vấn & Tìm kiếm (Search & Join)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_11_GetAllSongs_CheckJoin_ReturnsUploaderName()
        {
            // 1. Arrange: Tạo 1 bài hát để test JOIN
            string songName = "Join Test Song";
            _songDAL.InsertSong(songName, 1, "join.mp3", 1); // UserID=1 là "Nguyễn Văn Admin"

            // 2. Act: Lấy danh sách bài hát
            var songs = _songService.GetAllSongs(songName);

            // 3. Assert: Kiểm tra xem tầng DAL có lấy được tên người đăng từ bảng tbl_Users không
            Assert.NotEmpty(songs);
            // FullName trong SongDTO chính là UploaderName từ SQL
            Assert.Equal("Nguyễn Văn Admin", songs[0].FullName);

            CleanUpSong(songName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_12_SearchSong_ByTitle_ReturnsFilteredResults()
        {
            // Arrange: Tạo 2 bài hát khác nhau
            _songDAL.InsertSong("Sơn Tùng M-TP", 1, "mtp.mp3", 1);
            _songDAL.InsertSong("Đen Vâu", 1, "den.mp3", 1);

            // Act: Tìm kiếm bài hát có chữ "Sơn Tùng"
            var results = _songService.GetAllSongs("Sơn Tùng");

            // Assert
            Assert.All(results, s => Assert.Contains("Sơn Tùng", s.SongName));
            Assert.DoesNotContain(results, s => s.SongName == "Đen Vâu");

            CleanUpSong("Sơn Tùng M-TP");
            CleanUpSong("Đen Vâu");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_13_GetAllSongs_CheckFileUrl_Formatting()
        {
            // Arrange
            string songName = "Format Test";
            _songDAL.InsertSong(songName, 1, "music.mp3", 1);

            // Act
            var song = _songService.GetAllSongs(songName).FirstOrDefault();

            // Assert: Kiểm tra logic cộng chuỗi "/uploads/" trong Service
            Assert.NotNull(song);
            Assert.Equal("/uploads/music.mp3", song.FileUrl);

            CleanUpSong(songName);
        }

        #endregion

        #region Kiểm tra Ràng buộc (Constraint)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_14_InsertSong_InvalidGenre_ThrowsSqlException()
        {
            // Act & Assert: Thử chèn ID thể loại không tồn tại (999) 
            // SQL sẽ ném lỗi Foreign Key và C# bắt lại dưới dạng SqlException
            Assert.Throws<SqlException>(() => _songDAL.InsertSong("Lỗi Genre", 999, "err.mp3", 1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_15_InsertSong_SpecialCharacters_PreservesUnicode()
        {
            // Arrange: Tên bài hát có dấu tiếng Việt
            string songName = "Bài Hát Tiếng Việt Có Dấu ♫";
            CleanUpSong(songName);

            // Act
            _songDAL.InsertSong(songName, 1, "vn.mp3", 1);
            var song = _songService.GetAllSongs(songName).FirstOrDefault();

            // Assert: Kiểm tra SQL có lưu đúng Unicode (NVARCHAR) không
            Assert.NotNull(song);
            Assert.Equal(songName, song.SongName);

            CleanUpSong(songName);
        }

        #endregion

        /// <summary>
        /// Hàm dọn dẹp bài hát sau khi test
        /// </summary>
        private void CleanUpSong(string songName)
        {
            using (SqlConnection conn = new SqlConnection(TestConfig.ConnectionString))
            {
                string sql = "DELETE FROM tbl_Songs WHERE sSongName = @name";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", songName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}