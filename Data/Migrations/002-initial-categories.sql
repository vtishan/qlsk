INSERT dbo.DanhMucSuKien (TenDanhMuc, MoTa, TrangThai)
SELECT c.Name, c.Description, N'Hien'
FROM (VALUES
    (N'Âm nhạc', N'Hòa nhạc, đêm diễn và chương trình âm nhạc.'),
    (N'Nghệ thuật', N'Triển lãm, sân khấu và múa.'),
    (N'Workshop', N'Lớp thực hành và giao lưu theo chủ đề.'),
    (N'Hội nghị', N'Diễn đàn, hội thảo và gặp gỡ chuyên ngành.'),
    (N'Thể thao', N'Giải đấu và hoạt động vận động cộng đồng.')
) c(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.DanhMucSuKien d WHERE d.TenDanhMuc = c.Name);
