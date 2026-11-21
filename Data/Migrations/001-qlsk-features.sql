-- Additive changes for existing QLSK databases. New databases also create these from EF.
IF COL_LENGTH('dbo.YeuCauThue', 'NgayTao') IS NULL
    ALTER TABLE dbo.YeuCauThue ADD NgayTao datetime2(3) NULL;
IF COL_LENGTH('dbo.YeuCauThue', 'NoiDungYeuCau') IS NULL
    ALTER TABLE dbo.YeuCauThue ADD NoiDungYeuCau nvarchar(max) NULL;
IF COL_LENGTH('dbo.DonHang', 'KhoaTaoDon') IS NULL
    ALTER TABLE dbo.DonHang ADD KhoaTaoDon nvarchar(64) NULL;
IF COL_LENGTH('dbo.DonHang', 'DauVanTayTaoDon') IS NULL
    ALTER TABLE dbo.DonHang ADD DauVanTayTaoDon nvarchar(64) NULL;
IF COL_LENGTH('dbo.TaiKhoan', 'SuKienYeuThich') IS NULL
    ALTER TABLE dbo.TaiKhoan ADD SuKienYeuThich nvarchar(max) NULL;
IF COL_LENGTH('dbo.TaiKhoan', 'SuKienDaXem') IS NULL
    ALTER TABLE dbo.TaiKhoan ADD SuKienDaXem nvarchar(max) NULL;
