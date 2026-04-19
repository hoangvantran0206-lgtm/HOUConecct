using Microsoft.Data.SqlClient;
using System.Data;

namespace HOUConnect.Data.Repositories
{
    // Thêm từ khóa public ở đây
    public class UserDAL
    {
        private readonly SqlHelper _sqlHelper;

        // Constructor cũng phải là public
        public UserDAL(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public bool RegisterUser(string fullName, string email, string password)
        {
            // Thiết lập các tham số cho Stored Procedure
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@sFullName", fullName),
                new SqlParameter("@sEmail", email),
                new SqlParameter("@sPassword", password)
            };

            // Thực thi qua SqlHelper
            int rowsAffected = _sqlHelper.ExecuteNonQuery("sp_InsertUser", parameters);

            // Nếu số dòng bị tác động > 0 nghĩa là đã chèn thành công
            return rowsAffected > 0;
        }
        public DataTable GetUserByEmail(string email)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
               new SqlParameter("@sEmail", email)
            };
            return _sqlHelper.ExecuteQuery("sp_GetUserByEmail", parameters);
        }
        public DataTable GetAllUsers(string search)
        {
            // Nếu search bị null thì gán bằng chuỗi rỗng để tránh lỗi SQL
            search = search ?? "";

            SqlParameter[] paras = {
        new SqlParameter("@sSearchTerm", search)
                                     };

            // Gọi Stored Procedure sp_GetAllUsers đã tạo ở bước trước
            return _sqlHelper.ExecuteQuery("sp_GetAllUsers", paras);
        }
        public DataTable GetUserByID(int userId)
        {
            SqlParameter[] paras = { new SqlParameter("@iUserID", userId) };
            return _sqlHelper.ExecuteQuery("sp_GetUserByID", paras);
        }

        public bool UpdateUserStatus(int userId, int newStatus)
        {
            SqlParameter[] paras = {
        new SqlParameter("@iUserID", userId),
        new SqlParameter("@iNewStatus", newStatus)
    };
            return _sqlHelper.ExecuteNonQuery("sp_UpdateUserStatus", paras) > 0;
        }
        public DataTable Login(string email, string password)
        {
            // Sử dụng SqlParameter để chống SQL Injection (rất quan trọng cho bảo mật!)
            SqlParameter[] paras = {
        new SqlParameter("@sEmail", email),
        new SqlParameter("@sPassword", password)
    };

            // Gọi đúng tên Stored Procedure sp_Login mà chúng ta đã thống nhất
            return _sqlHelper.ExecuteQuery("sp_Login", paras);
        }

    }
}