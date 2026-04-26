using HOUConnect.Data.Models;
using HOUConnect.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOUConnect.Business.Services
{
    public class UserService
    {
        private readonly UserDAL _userDAL;

        public UserService(UserDAL userDAL)
        {
            _userDAL = userDAL;
        }
        public string CreateAccount(string name, string email, string password)
        {
            email = email?.Trim() ?? "";
            password = password?.Trim() ?? "";
            // 1. Kiểm tra email có phải của HOU không
            if (!email.ToLower().EndsWith("@hou.edu.vn"))
            {
                return "Hệ thống chỉ dành riêng cho sinh viên Đại học Mở Hà Nội!";
            }

            // 2. Kiểm tra độ dài mật khẩu (ví dụ tối thiểu 6 ký tự)
            if (password.Length < 6)
            {
                return "Mật khẩu quá ngắn, vui lòng nhập trên 6 ký tự.";
            }

            // 3. Gọi xuống tầng Data để lưu
            bool isSuccess = _userDAL.RegisterUser(name, email, password);

            return isSuccess ? "Success" : "Đã có lỗi xảy ra trong quá trình đăng ký.";
        }
        public string AuthenticateUser(string email, string password)
        {
            // 1. Lấy dữ liệu từ DAL
            DataTable dt = _userDAL.GetUserByEmail(email);

            if (dt.Rows.Count > 0)
            {
                string dbPassword = dt.Rows[0]["sPassword"].ToString();

                // 2. So sánh mật khẩu (Tạm thời là so sánh chuỗi, tí nữa sẽ nói về Hashing)
                if (dbPassword == password)
                {
                    return "Success";
                }
            }
            return "Email hoặc mật khẩu không chính xác!";
        }
        public List<UserDTO> GetUsers(string? search)
        {
            // Gọi xuống DAL để lấy DataTable
            DataTable dt = _userDAL.GetAllUsers(search ?? "");
            List<UserDTO> list = new List<UserDTO>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new UserDTO
                {
                    UserID = Convert.ToInt32(dr["PK_iUserID"]),
                    FullName = dr["sFullName"].ToString() ?? "",
                    Email = dr["sEmail"].ToString() ?? "",
                    Status = Convert.ToInt32(dr["iStatus"]),
                    CreatedAt = Convert.ToDateTime(dr["dCreatedAt"])
                });
            }
            return list;
        }
        public void ToggleUserLock(int userId)
        {
            DataTable dt = _userDAL.GetUserByID(userId);
            if (dt.Rows.Count > 0)
            {
                int currentStatus = Convert.ToInt32(dt.Rows[0]["iStatus"]);
                int newStatus = (currentStatus == 1) ? 0 : 1;
                _userDAL.UpdateUserStatus(userId, newStatus);
            }
        }
        public UserDTO? CheckLogin(string email, string password)
        {
            email = email?.Trim() ?? "";
            password = password?.Trim() ?? "";
            // Gọi xuống DAL để thực thi Procedure
            DataTable dt = _userDAL.Login(email, password);

            if (dt.Rows.Count > 0)
            {
                return new UserDTO
                {
                    UserID = Convert.ToInt32(dt.Rows[0]["PK_iUserID"]),
                    FullName = dt.Rows[0]["sFullName"].ToString() ?? "",
                    RoleID = Convert.ToInt32(dt.Rows[0]["iRole"]) // 1: Admin, 0: User
                };
            }
            return null; // Đăng nhập thất bại
        }
        public bool ChangeAccountStatus(int userId, int status)
        {
            // Gọi trực tiếp xuống DAL để cập nhật
            return _userDAL.UpdateUserStatus(userId, status);
        }
    }

}
