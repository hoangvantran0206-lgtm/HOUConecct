using System;

namespace HOUConnect.Data.Models
{
    // Class chứa thông tin chi tiết bài hát
    public class SongDTO
    {
      
        public int SongID { get; set; }
        public string SongName { get; set; } = string.Empty;

        // Lưu ID để làm việc với DB
        public int GenreID { get; set; }
        // Lưu tên để hiển thị lên giao diện (ví dụ: "Pop", "Rock")
        public string GenreName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;
        public int UserID { get; set; }
        public DateTime UploadDate { get; set; }
        public int Status { get; set; }
    
        public string FullName { get; set; } = string.Empty; // Để hiện tên người upload
         
        public DateTime CreatedAt { get; set; }
    }
    

    // Class bổ trợ cho việc lấy danh sách Thể loại đổ vào Dropdown
    public class GenreDTO
    {
        public int GenreID { get; set; }
        public string GenreName { get; set; }
    }
}