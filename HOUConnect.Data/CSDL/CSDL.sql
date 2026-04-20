
CREATE TABLE tbl_Users (
    PK_iUserID INT IDENTITY(1,1) PRIMARY KEY, -- Mã định danh tự tăng
    sFullName NVARCHAR(100) NOT NULL,        -- Họ tên sinh viên
    sEmail NVARCHAR(100) NOT NULL UNIQUE,     -- Email @hou.edu.vn (Duy nhất)
    sPassword NVARCHAR(255) NOT NULL,        -- Mật khẩu (Sau này sẽ lưu Hash)
    sAvatar NVARCHAR(255) DEFAULT 'default.png', 
    iStatus INT DEFAULT 1,                   -- 1: Hoạt động, 0: Bị khóa
    iRole INT DEFAULT 0,                      -- 0: Sinh viên, 1: Admin
    dCreatedAt DATETIME DEFAULT GETDATE()    -- Ngày tham gia
);

-- 2. Tạo SP Đăng ký
CREATE PROCEDURE sp_InsertUser
    @sFullName NVARCHAR(100),
    @sEmail NVARCHAR(100),
    @sPassword NVARCHAR(255)
AS
BEGIN
    -- Mặc định khi đăng ký là iStatus = 1 (Hoạt động)
    INSERT INTO tbl_Users (sFullName, sEmail, sPassword, iStatus, iRole)
    VALUES (@sFullName, @sEmail, @sPassword, 1, 0);
END
 --3. SP Lấy thông tin để Đăng nhập 
 CREATE PROCEDURE sp_GetUserByEmail
    @sEmail NVARCHAR(100)
AS
BEGIN
    SELECT * FROM tbl_Users 
    WHERE sEmail = @sEmail AND iStatus = 1; -- Chỉ lấy người dùng chưa bị khóa
END
-- -- Lấy thông tin người dùng nếu tài khoản đang hoạt động (iStatus = 1)
CREATE OR ALTER PROCEDURE sp_Login
    @sEmail NVARCHAR(100),
    @sPassword NVARCHAR(100)
AS
BEGIN
    SELECT PK_iUserID, sFullName, iRole 
    FROM tbl_Users 
    WHERE sEmail = @sEmail AND sPassword = @sPassword AND iStatus = 1
END
 -- 4. Tạo bảng thể loại nhạc 
 CREATE TABLE tbl_Genres (
    PK_iGenreID INT IDENTITY(1,1) PRIMARY KEY, -- Mã thể loại
    sGenreName NVARCHAR(50) NOT NULL UNIQUE,   -- Tên thể loại (Pop, Rock,...)
    sDescription NVARCHAR(255)                 -- Mô tả ngắn (không bắt buộc)
);

-- Chèn sẵn một số dữ liệu mẫu
INSERT INTO tbl_Genres (sGenreName) VALUES (N'Pop'), (N'Ballad'), (N'Rock'), (N'EDM'), (N'Lo-fi');

--5 .Bảng bài hát 
CREATE TABLE tbl_Songs (
    PK_iSongID INT IDENTITY(1,1) PRIMARY KEY,
    sSongName NVARCHAR(200) NOT NULL,
    FK_iGenreID INT NOT NULL,                -- Khóa ngoại trỏ sang tbl_Genres
    sFileUrl NVARCHAR(255) NOT NULL,
    FK_iUserID INT NOT NULL,                 -- Khóa ngoại trỏ sang tbl_Users
    dUploadDate DATETIME DEFAULT GETDATE(),
    iStatus INT DEFAULT 1,
    
    -- Các ràng buộc khóa ngoại
    CONSTRAINT FK_Songs_Genres FOREIGN KEY (FK_iGenreID) REFERENCES tbl_Genres(PK_iGenreID),
    CONSTRAINT FK_Songs_Users FOREIGN KEY (FK_iUserID) REFERENCES tbl_Users(PK_iUserID)
);
--Thủ tục lưu bài hát (sp_InsertSong)
CREATE PROCEDURE sp_InsertSong
    @sSongName NVARCHAR(200),
    @iGenreID INT,        -- Truyền ID thay vì truyền chữ
    @sFileUrl NVARCHAR(255),
    @iUserID INT
AS
BEGIN
    INSERT INTO tbl_Songs (sSongName, FK_iGenreID, sFileUrl, FK_iUserID)
    VALUES (@sSongName, @iGenreID, @sFileUrl, @iUserID);
END
--Thủ tục lấy tất cả thể loại của bài hát 
CREATE  PROCEDURE sp_GetAllGenres
AS
BEGIN
    SELECT PK_iGenreID, sGenreName FROM tbl_Genres;
END

--  Stored Procedure lấy danh sách người dùng , hỗ trợ tìm kiếm theo tên , email
CREATE OR ALTER PROCEDURE sp_GetAllUsers
    @sSearchTerm NVARCHAR(100) = '' -- Thêm tham số tìm kiếm
AS
BEGIN
    SELECT PK_iUserID, sFullName, sEmail, iStatus, dCreatedAt 
    FROM tbl_Users
    WHERE iRole = 0 -- Chỉ quản lý sinh viên
      AND (sFullName LIKE '%' + @sSearchTerm + '%' OR sEmail LIKE '%' + @sSearchTerm + '%')
    ORDER BY dCreatedAt DESC;
END
-- SP cập nhật trạng thái người dùng 
-- 2. Cập nhật trạng thái
CREATE OR ALTER PROCEDURE sp_UpdateUserStatus
    @iUserID INT,
    @iNewStatus INT
AS
BEGIN
    UPDATE tbl_Users SET iStatus = @iNewStatus WHERE PK_iUserID = @iUserID;
END
--sp lấy người dùng theo id 
CREATE OR ALTER PROCEDURE sp_GetUserByID
    @iUserID INT
AS
BEGIN
    SELECT * FROM tbl_Users WHERE PK_iUserID = @iUserID;
END
--thủ tục lấy tất cả bài hát 
CREATE PROCEDURE sp_GetAllSongs
AS
BEGIN
    SELECT 
        s.PK_iSongID, 
        s.sSongName, 
        s.sFileUrl, 
        u.sFullName AS UploaderName, -- Lấy tên người đăng từ bảng Users
        s.dUploadDate
    FROM tbl_Songs s
    INNER JOIN tbl_Users u ON s.FK_iUserID = u.PK_iUserID
    WHERE s.iStatus = 1 -- Chỉ lấy những bài hát đang hoạt động
    ORDER BY s.dUploadDate DESC;
END