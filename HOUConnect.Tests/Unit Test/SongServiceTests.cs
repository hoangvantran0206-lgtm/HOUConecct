using Xunit;
using Moq;
using System.Data;
using System.Collections.Generic;
using HOUConnect.Business.Services;
using HOUConnect.Data.Repositories;
using HOUConnect.Data.Models;
using System;

namespace HOUConnect.Tests
{
    public class SongServiceTests
    {
        private readonly Mock<SongDAL> _mockSongDAL;
        private readonly SongService _songService;

        public SongServiceTests()
        {
            // Arrange: Khởi tạo đối tượng giả lập cho tầng truy xuất dữ liệu (DAL)
            // và "tiêm" (Inject) vào lớp nghiệp vụ SongService.
            _mockSongDAL = new Mock<SongDAL>(null);
            _songService = new SongService(_mockSongDAL.Object);
        }

        /// <summary>
        /// Kiểm tra tính năng lấy danh sách thể loại nhạc.
        /// Xác minh dữ liệu từ DataTable được ánh xạ (mapping) chính xác sang danh sách GenreDTO.
        /// </summary>
        [Fact]
        public void GetGenres_ValidData_ReturnsMappedList()
        {
            // Arrange: Giả lập 2 thể loại nhạc trong Database
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iGenreID", typeof(int));
            dt.Columns.Add("sGenreName", typeof(string));
            dt.Rows.Add(1, "Pop");
            dt.Rows.Add(2, "Rock");

            _mockSongDAL.Setup(x => x.GetGenres()).Returns(dt);

            // Act: Gọi hàm lấy danh sách thể loại từ tầng Service
            var result = _songService.GetGenres();

            // Assert: Kiểm tra số lượng phần tử và dữ liệu có khớp không
            Assert.Equal(2, result.Count);
            Assert.Equal("Pop", result[0].GenreName);
        }

        /// <summary>
        /// Kiểm tra ràng buộc dữ liệu: Tên bài hát không được để trống khi tải lên.
        /// </summary>
        [Fact]
        public void ValidateAndUpload_EmptyName_ReturnsErrorMessage()
        {
            // Act: Thử tải lên bài hát với tham số tên là chuỗi rỗng
            var result = _songService.ValidateAndUpload("", 1, "song.mp3", 1);

            // Assert: Hệ thống phải phát hiện lỗi và trả về thông báo tương ứng
            Assert.Equal("Tên bài hát không được để trống!", result);
        }

        /// <summary>
        /// Kiểm tra ràng buộc dữ liệu: Người dùng bắt buộc phải chọn thể loại nhạc (ID > 0).
        /// </summary>
        [Fact]
        public void ValidateAndUpload_InvalidGenre_ReturnsErrorMessage()
        {
            // Act: Thử tải lên với mã thể loại không hợp lệ (bằng 0)
            var result = _songService.ValidateAndUpload("Nắng ấm xa dần", 0, "song.mp3", 1);

            // Assert: Hệ thống phải yêu cầu người dùng chọn thể loại
            Assert.Equal("Vui lòng chọn thể loại!", result);
        }

        /// <summary>
        /// Kiểm tra luồng tải lên thành công khi mọi thông tin đều hợp lệ.
        /// Xác minh logic tương tác thành công giữa tầng Service và tầng DAL.
        /// </summary>
        [Fact]
        public void ValidateAndUpload_Success_ReturnsSuccess()
        {
            // Arrange: Giả lập tầng DAL thực hiện lưu dữ liệu vào SQL thành công (trả về true)
            _mockSongDAL.Setup(x => x.InsertSong(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                        .Returns(true);

            // Act: Thực hiện tải lên một bài hát hợp lệ
            var result = _songService.ValidateAndUpload("Lạc trôi", 1, "lactroi.mp3", 1);

            // Assert: Kết quả trả về phải là chuỗi "Success"
            Assert.Equal("Success", result);
        }

        /// <summary>
        /// Kiểm tra logic xử lý đường dẫn file bài hát.
        /// Xác minh hệ thống tự động thêm tiền tố "/uploads/" vào URL trước khi hiển thị trên giao diện.
        /// </summary>
        [Fact]
        public void GetAllSongs_MappingPath_AddsUploadPrefix()
        {
            // Arrange: Giả lập dữ liệu bài hát lấy từ Database
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iSongID", typeof(int));
            dt.Columns.Add("sSongName", typeof(string));
            dt.Columns.Add("UploaderName", typeof(string));
            dt.Columns.Add("sFileUrl", typeof(string));
            dt.Columns.Add("dUploadDate", typeof(DateTime));
            dt.Rows.Add(1, "Making My Way", "Sơn Tùng", "mmw.mp3", DateTime.Now);

            _mockSongDAL.Setup(x => x.GetAllSongs(It.IsAny<string>())).Returns(dt);

            // Act: Lấy danh sách bài hát
            var result = _songService.GetAllSongs("Sơn Tùng");

            // Assert: Đường dẫn file phải được định dạng chuẩn để Trình phát nhạc (Player) có thể truy cập
            Assert.Equal("/uploads/mmw.mp3", result[0].FileUrl);
        }
        /// <summary>
        /// UT11: Kiểm tra tính khớp nối dữ liệu giữa Database và danh sách hiển thị.
        /// Đảm bảo ID và Tên thể loại không bị sai lệch sau khi mapping sang DTO.
        /// </summary>
        [Fact]
        public void GetGenres_DataIntegrity_MatchesDatabaseRecords()
        {
            // Arrange: Tạo một DataTable giả lập với 3 bản ghi cụ thể
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iGenreID", typeof(int));
            dt.Columns.Add("sGenreName", typeof(string));

            dt.Rows.Add(1, "Nhạc trẻ");
            dt.Rows.Add(2, "Remix");
            dt.Rows.Add(3, "Bolero");

            // Setup: Khi DAL được gọi, nó phải trả về đúng bảng 3 bản ghi này
            _mockSongDAL.Setup(x => x.GetGenres()).Returns(dt);

            // Act: Tầng Service thực hiện lấy dữ liệu và chuyển đổi sang List<GenreDTO>
            var result = _songService.GetGenres();

            // Assert: Kiểm tra 3 lớp bảo vệ
            // 1. Số lượng phải khớp (3 dòng DB = 3 phần tử List)
            Assert.Equal(3, result.Count);

            // 2. Kiểm tra tính chính xác của bản ghi đầu tiên
            Assert.Equal(1, result[0].GenreID);
            Assert.Equal("Nhạc trẻ", result[0].GenreName);

            // 3. Kiểm tra tính chính xác của bản ghi cuối cùng (để đảm bảo vòng lặp chạy hết)
            Assert.Equal(3, result[2].GenreID);
            Assert.Equal("Bolero", result[2].GenreName);
        }
        /// <summary>
        /// UT16: Kiểm tra tính năng tìm kiếm bài hát theo tiêu đề.
        /// Xác minh từ khóa được truyền đúng xuống DAL và danh sách kết quả được lọc chính xác.
        /// </summary>
        [Fact]
        public void GetAllSongs_SearchBySongName_ReturnsFilteredList()
        {
            // Arrange
            string searchTerm = "Lạc trôi";
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iSongID", typeof(int));
            dt.Columns.Add("sSongName", typeof(string));
            dt.Columns.Add("UploaderName", typeof(string));
            dt.Columns.Add("sFileUrl", typeof(string));
            dt.Columns.Add("dUploadDate", typeof(DateTime));
            dt.Rows.Add(1, "Lạc trôi", "Sơn Tùng M-TP", "lactroi.mp3", DateTime.Now);

            _mockSongDAL.Setup(x => x.GetAllSongs(searchTerm)).Returns(dt);

            // Act
            var result = _songService.GetAllSongs(searchTerm);

            // Assert
            Assert.Single(result); // Phải trả về đúng 1 bài
            Assert.Equal("Lạc trôi", result[0].SongName);
            // Verify: Đảm bảo DAL được gọi đúng 1 lần với từ khóa "Lạc trôi"
            _mockSongDAL.Verify(x => x.GetAllSongs(searchTerm), Times.Once);
        }

        /// <summary>
        /// UT18: Kiểm tra tính năng tìm kiếm bài hát theo tên người đăng (Uploader).
        /// Đảm bảo sinh viên có thể tìm thấy nhạc của bạn bè mình thông qua tên uploader.
        /// </summary>
        [Fact]
        public void GetAllSongs_SearchByUploader_ReturnsFilteredList()
        {
            // Arrange
            string uploaderSearch = "Hoàng IT";
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iSongID", typeof(int));
            dt.Columns.Add("sSongName", typeof(string));
            dt.Columns.Add("UploaderName", typeof(string));
            dt.Columns.Add("sFileUrl", typeof(string));
            dt.Columns.Add("dUploadDate", typeof(DateTime));
            dt.Rows.Add(1, "Em của ngày hôm qua", "Hoàng IT", "emcua.mp3", DateTime.Now);

            _mockSongDAL.Setup(x => x.GetAllSongs(uploaderSearch)).Returns(dt);

            // Act
            var result = _songService.GetAllSongs(uploaderSearch);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("Hoàng IT", result[0].FullName);
        }

    }
}