using Xunit;
using Moq;
using System.Data;
using HOUConnect.Business.Services;
using HOUConnect.Data.Repositories;

namespace HOUConnect.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserDAL> _mockUserDAL;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Khởi tạo đối tượng giả lập cho tầng Data và tiêm vào Service
            _mockUserDAL = new Mock<UserDAL>(null);
            _userService = new UserService(_mockUserDAL.Object);
        }

        /// <summary>
        /// Kiểm tra tính năng chặn đăng ký khi Email không thuộc domain @hou.edu.vn
        /// </summary>
        [Fact]
        public void CreateAccount_EmailNotHOU_ReturnsError()
        {
            // Act: Thử đăng ký với email Gmail
            var result = _userService.CreateAccount("Hoàng", "hoang@gmail.com", "123456");

            // Assert: Hệ thống phải trả về thông báo lỗi domain
            Assert.Equal("Hệ thống chỉ dành riêng cho sinh viên Đại học Mở Hà Nội!", result);
        }

        /// <summary>
        /// Kiểm tra tính năng chặn đăng ký khi mật khẩu ngắn hơn 6 ký tự
        /// </summary>
        [Fact]
        public void CreateAccount_PasswordTooShort_ReturnsErrorMessage()
        {
            // Act: Nhập mật khẩu chỉ có 3 ký tự
            var result = _userService.CreateAccount("Nguyễn Văn Hoàng", "hoang@hou.edu.vn", "123");

            // Assert: Hệ thống phải báo lỗi độ dài mật khẩu
            Assert.Equal("Mật khẩu quá ngắn, vui lòng nhập trên 6 ký tự.", result);
        }

        /// <summary>
        /// Kiểm tra luồng đăng ký thành công khi mọi dữ liệu đều hợp lệ
        /// </summary>
        [Fact]
        public void CreateAccount_Success_ReturnsSuccessMessage()
        {
            // Arrange: Giả lập DAL trả về true khi lưu dữ liệu
            _mockUserDAL.Setup(x => x.RegisterUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(true);

            // Act
            var result = _userService.CreateAccount("Hoàng IT", "hoang@hou.edu.vn", "password123");

            // Assert: Trả về Success
            Assert.Equal("Success", result);
        }

        /// <summary>
        /// Kiểm tra xử lý đăng nhập khi tài khoản hoặc mật khẩu không tồn tại trong DB
        /// </summary>
        [Fact]
        public void CheckLogin_UserNotFound_ReturnsNull()
        {
            // Arrange: Giả lập trả về DataTable rỗng (không tìm thấy user)
            _mockUserDAL.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(new DataTable());

            // Act
            var result = _userService.CheckLogin("unknown@hou.edu.vn", "123456");

            // Assert: Kết quả trả về phải là null
            Assert.Null(result);
        }

        /// <summary>
        /// Kiểm tra luồng đăng nhập thành công và chuyển đổi dữ liệu từ DataTable sang UserDTO
        /// </summary>
        [Fact]
        public void CheckLogin_ValidCredentials_ReturnsUserDTO()
        {
            // Arrange: Tạo DataTable giả lập chứa 1 bản ghi người dùng
            DataTable dt = new DataTable();
            dt.Columns.Add("PK_iUserID", typeof(int));
            dt.Columns.Add("sFullName", typeof(string));
            dt.Columns.Add("iRole", typeof(int));
            dt.Rows.Add(1, "Nguyễn Văn Hoàng", 1);

            _mockUserDAL.Setup(x => x.Login("hoang@hou.edu.vn", "123456")).Returns(dt);

            // Act
            var result = _userService.CheckLogin("hoang@hou.edu.vn", "123456");

            // Assert: Đối tượng trả về không null và khớp thông tin ID, Tên
            Assert.NotNull(result);
            Assert.Equal("Nguyễn Văn Hoàng", result.FullName);
            Assert.Equal(1, result.UserID);
        }

    

        /// <summary>
        /// Kiểm tra tính năng quản trị: Khóa hoặc mở tài khoản sinh viên
        /// </summary>
        [Fact]
        public void UpdateUserStatus_ValidAction_ReturnsTrue()
        {
            // Arrange: Giả lập DAL thực thi câu lệnh SQL thành công
            _mockUserDAL.Setup(x => x.UpdateUserStatus(1, 0)).Returns(true);

            // Act: Admin thực hiện khóa tài khoản (status = 0)
            bool result = _userService.ChangeAccountStatus(1, 0);

            // Assert: Trả về kết quả thành công
            Assert.True(result);
        }
    }
}