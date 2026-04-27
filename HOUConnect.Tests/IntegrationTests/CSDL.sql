-- 1. Tạo Database Test (Xóa nếu đã tồn tại để làm mới môi trường)
USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'HOUConnect_Test')
BEGIN
    ALTER DATABASE HOUConnect_Test SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HOUConnect_Test;
END
GO

CREATE DATABASE HOUConnect_Test;
GO

USE HOUConnect_Test;
GO

-- 2. Tạo các bảng (Dựa trên cấu trúc sếp đã thiết kế)
CREATE TABLE tbl_Users (
    PK_iUserID INT IDENTITY(1,1) PRIMARY KEY,
    sFullName NVARCHAR(100) NOT NULL,
    sEmail NVARCHAR(100) NOT NULL UNIQUE,
    sPassword NVARCHAR(255) NOT NULL,
    sAvatar NVARCHAR(255) DEFAULT 'default.png',
    iStatus INT DEFAULT 1,
    iRole INT DEFAULT 0,
    dCreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE tbl_Genres (
    PK_iGenreID INT IDENTITY(1,1) PRIMARY KEY,
    sGenreName NVARCHAR(50) NOT NULL UNIQUE,
    sDescription NVARCHAR(255)
);

CREATE TABLE tbl_Songs (
    PK_iSongID INT IDENTITY(1,1) PRIMARY KEY,
    sSongName NVARCHAR(200) NOT NULL,
    FK_iGenreID INT NOT NULL,
    sFileUrl NVARCHAR(255) NOT NULL,
    FK_iUserID INT NOT NULL,
    dUploadDate DATETIME DEFAULT GETDATE(),
    iStatus INT DEFAULT 1,
    CONSTRAINT FK_Songs_Genres FOREIGN KEY (FK_iGenreID) REFERENCES tbl_Genres(PK_iGenreID),
    CONSTRAINT FK_Songs_Users FOREIGN KEY (FK_iUserID) REFERENCES tbl_Users(PK_iUserID)
);

GO

-- 3. Đổ dữ liệu mẫu (Seed Data) để phục vụ Integration Test
-- Tạo 1 Admin và 1 Sinh viên mẫu
INSERT INTO tbl_Users (sFullName, sEmail, sPassword, iRole)
VALUES 
(N'Nguyễn Văn Admin', 'admin@hou.edu.vn', 'hash_password_123', 1),
(N'Trần Văn Sinh Viên', 'student@hou.edu.vn', 'hash_password_456', 0);

-- Tạo các thể loại nhạc cơ bản
INSERT INTO tbl_Genres (sGenreName, sDescription)
VALUES 
(N'Pop', N'Nhạc trẻ phổ biến'),
(N'Ballad', N'Nhạc trữ tình nhẹ nhàng'),
(N'Podcast', N'Nội dung chia sẻ âm thanh');
GO
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
--hỗ trợ tìm kiếm bài hát 
Create or ALTER PROCEDURE sp_GetAllSongs
    @sSearchTerm NVARCHAR(100) = '' 
AS
BEGIN
    SELECT 
        s.PK_iSongID, 
        s.sSongName, 
        s.sFileUrl, 
        u.sFullName AS UploaderName,
        s.dUploadDate
    FROM tbl_Songs s
    INNER JOIN tbl_Users u ON s.FK_iUserID = u.PK_iUserID
    WHERE s.iStatus = 1 
      -- Phải dùng N để hỗ trợ tiếng Việt và % để tìm kiếm mẫu
      AND (s.sSongName LIKE N'%' + ISNULL(@sSearchTerm, '') + '%' 
           OR u.sFullName LIKE N'%' + ISNULL(@sSearchTerm, '') + '%')
    ORDER BY s.dUploadDate DESC;
END