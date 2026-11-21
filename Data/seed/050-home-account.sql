INSERT dbo.Banner(TieuDe,HinhAnh,DuongDan,ThuTu,TrangThai)
SELECT e.TenSuKien, e.AnhBia, CONCAT('#event/', e.Id), b.Position, b.Visibility
FROM (VALUES ('upcoming',1,N'Hien'), ('hanoi',2,N'Hien'), ('other',3,N'Hien'), ('completed',4,N'An')) b(Code,Position,Visibility)
JOIN #SeedEvent k ON k.Code=b.Code JOIN dbo.SuKien e ON e.Id=k.Id;

DECLARE @upcoming int = (SELECT Id FROM #SeedEvent WHERE Code='upcoming');
DECLARE @live int = (SELECT Id FROM #SeedEvent WHERE Code='live');
DECLARE @hanoi int = (SELECT Id FROM #SeedEvent WHERE Code='hanoi');
UPDATE t SET SuKienYeuThich=JSON_ARRAY(@upcoming,@hanoi), SuKienDaXem=JSON_ARRAY(@live,@hanoi,@upcoming)
FROM dbo.TaiKhoan t JOIN #SeedAccount a ON a.Id=t.Id WHERE a.Code='customer';
