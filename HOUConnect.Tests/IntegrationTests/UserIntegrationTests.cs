using Xunit;
using HOUConnect.Data.Repositories;
using HOUConnect.Business.Services;
using HOUConnect.Data.Models;
using HOUConnect.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HOUConnect.Tests.IntegrationTests
{
    /// <summary>
    /// Lớp kiểm thử tích hợp cho các chức năng liên quan đến Người dùng
    /// Đảm bảo sự phối hợp chính xác giữa Service -> DAL -> SQL Server
    /// </summary>
    public class UserIntegrationTests
    {
        private readonly UserDAL _userDAL;
        private readonly UserService _userService;
        private readonly SqlHelper _sqlHelper;

        public UserIntegrationTests()
        {
            // KHỞI TẠO: Sử dụng đối tượng THẬT và Database TEST để kiểm thử tích hợp
            _sqlHelper = new SqlHelper(TestConfig.ConnectionString);
            _userDAL = new UserDAL(_sqlHelper);
            _userService = new UserService(_userDAL);
        }

        #region Chức năng Đăng ký (CreateAccount)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_01_CreateAccount_ValidHOUEmail_SavesToDB()
        {
            // 1. Arrange: Chuẩn bị dữ liệu mẫu hợp lệ (Email đuôi @hou.edu.vn)
            string email = "test_hou@hou.edu.vn";
            CleanUpUser(email); // Đảm bảo môi trường sạch trước khi test

            // 2. Act: Gọi hàm nghiệp vụ tạo tài khoản
            var result = _userService.CreateAccount("Sếp Hoàng Test", email, "password123");

            // 3. Assert: Kiểm tra kết quả
            Assert.Equal("Success", result); // Phải trả về chuỗi "Success"

            // Kiểm tra thực tế trong DB xem dữ liệu đã được lưu chưa
            DataTable dt = _userDAL.GetUserByEmail(email);
            Assert.Single(dt.Rows); // Phải tồn tại duy nhất 1 bản ghi

            CleanUpUser(email); // Dọn dẹp dữ liệu rác sau khi test xong
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_02_CreateAccount_NonHOUEmail_ReturnsErrorMessage()
        {
            // Act: Thử đăng ký với email không thuộc domain @hou.edu.vn
            var result = _userService.CreateAccount("User Ngoai", "user@gmail.com", "password123");

            // Assert: Hệ thống phải chặn và trả về thông báo lỗi quy định
            Assert.Equal("Hệ thống chỉ dành riêng cho sinh viên Đại học Mở Hà Nội!", result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_03_CreateAccount_ShortPassword_ReturnsErrorMessage()
        {
            // Act: Thử đăng ký với mật khẩu chỉ có 3 ký tự
            var result = _userService.CreateAccount("Hoàng HOU", "hoang@hou.edu.vn", "123");

            // Assert: Hệ thống phải báo lỗi mật khẩu ngắn (dưới 6 ký tự)
            Assert.Equal("Mật khẩu quá ngắn, vui lòng nhập trên 6 ký tự.", result);
        }

        #endregion

        #region Chức năng Đăng nhập (CheckLogin)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_04_CheckLogin_CorrectCredentials_ReturnsUserDTO()
        {
            // 1. Arrange: Tạo sẵn một tài khoản thật trong DB để test đăng nhập
            string email = "login_success@hou.edu.vn";
            string pass = "password123";
            _userDAL.RegisterUser("Login Success", email, pass);

            // 2. Act: Thực hiện gọi hàm đăng nhập
            var user = _userService.CheckLogin(email, pass);

            // 3. Assert: Đăng nhập thành công phải trả về đối tượng UserDTO chứa đúng thông tin
            Assert.NotNull(user);
            Assert.Equal("Login Success", user.FullName);

            CleanUpUser(email); // Dọn dẹp dữ liệu mẫu
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_05_CheckLogin_WrongPassword_ReturnsNull()
        {
            // 1. Arrange: Tạo tài khoản nhưng khi gọi Act sẽ truyền sai mật khẩu
            string email = "wrong_pass@hou.edu.vn";
            _userDAL.RegisterUser("Wrong Pass", email, "correct_pass");

            // 2. Act: Đăng nhập với mật khẩu không khớp
            var user = _userService.CheckLogin(email, "incorrect_pass");

            // 3. Assert: Kết quả trả về phải là null (Đăng nhập thất bại)
            Assert.Null(user);

            CleanUpUser(email);
        }

        #endregion

        #region Quản lý tài khoản (Lock/Search)

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_06_ToggleUserLock_ChangesStatusInDatabase()
        {
            // 1. Arrange: Tạo user có trạng thái Hoạt động (iStatus = 1)
            string email = "lock_test@hou.edu.vn";
            _userDAL.RegisterUser("Lock Test", email, "123456");
            DataTable dtInitial = _userDAL.GetUserByEmail(email);
            int userId = Convert.ToInt32(dtInitial.Rows[0]["PK_iUserID"]);

            // 2. Act: Thực hiện đảo ngược trạng thái (Khóa tài khoản)
            _userService.ToggleUserLock(userId);

            // 3. Assert: Kiểm tra trong Database xem Status đã chuyển sang 0 chưa
            DataTable dtAfter = _userDAL.GetUserByID(userId);
            Assert.Equal(0, Convert.ToInt32(dtAfter.Rows[0]["iStatus"]));

            CleanUpUser(email);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IT_07_GetUsers_SearchByFullName_ReturnsMatch()
        {
            // 1. Arrange: Chuẩn bị dữ liệu có tên chứa từ khóa cần tìm
            string email = "search_test@hou.edu.vn";
            _userDAL.RegisterUser("Tim Kiem Hoang", email, "123456");

            // 2. Act: Tìm kiếm với từ khóa "Hoang"
            var list = _userService.GetUsers("Hoang");

            // 3. Assert: Danh sách trả về không được rỗng và phải chứa User vừa tạo
            Assert.NotEmpty(list);
            Assert.Contains(list, u => u.FullName.Contains("Hoang"));

            CleanUpUser(email);
        }

        #endregion

        /// <summary>
        /// Hàm dọn dẹp dữ liệu kiểm thử (Data Isolation)
        /// Đảm bảo các bài test không bị lỗi do trùng lặp dữ liệu từ những lần chạy trước
        /// </summary>
        private void CleanUpUser(string email)
        {
            using (SqlConnection conn = new SqlConnection(TestConfig.ConnectionString))
            {
                // Sử dụng SQL thô để DELETE thay vì Stored Procedure để linh hoạt hơn trong việc dọn dẹp
                string sql = "DELETE FROM tbl_Users WHERE sEmail = @email";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}