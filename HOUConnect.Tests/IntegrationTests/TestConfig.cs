using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOUConnect.Tests.IntegrationTests
{
    public static class TestConfig
    {
     
    
       
            // Trỏ thẳng vào DB HOUConnect_Test mà sếp vừa chạy script lúc nãy
     public const string ConnectionString = "Server=HOANG;Database=HOUConnect_Test;Trusted_Connection=True;TrustServerCertificate=True;";
       
   

    // Sếp có thể thêm các cấu hình giả lập khác tại đây
    public const string TestUploadFolder = "C:\\Temp\\HOU_Uploads_Test\\";
    }
}