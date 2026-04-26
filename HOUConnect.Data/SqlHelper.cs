using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HOUConnect.Data
{
    public class SqlHelper
    {
        private readonly string _connString;

        public SqlHelper(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection");
        }

        // Dùng cho INSERT, UPDATE, DELETE
        public int ExecuteNonQuery(string spName, SqlParameter[] paras)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                SqlCommand cmd = new SqlCommand(spName, conn) { CommandType = CommandType.StoredProcedure };
                if (paras != null) cmd.Parameters.AddRange(paras);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Dùng cho SELECT (Lấy dữ liệu)
        public DataTable ExecuteQuery(string spName, SqlParameter[] paras)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                SqlCommand cmd = new SqlCommand(spName, conn) { CommandType = CommandType.StoredProcedure };
                if (paras != null) cmd.Parameters.AddRange(paras);
                foreach (var p in paras)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL gọi: {spName} với tham số {p.ParameterName} = {p.Value}");
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}