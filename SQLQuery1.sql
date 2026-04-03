CREATE DATABASE BatDongSanDB;
GO
USE BatDongSanDB;
GO
CREATE TABLE BatDongSans (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(250),
    DiaChi NVARCHAR(500),
    Gia DECIMAL(18,2),
    LoaiHinh NVARCHAR(100)
);
GO
-- Chèn thử 1 dòng dữ liệu mẫu
INSERT INTO BatDongSans (TieuDe, DiaChi, Gia, LoaiHinh) 
VALUES (N'Căn hộ cao cấp Vinhomes', N'Bình Thạnh, TP.HCM', 4500000000, N'Căn hộ');