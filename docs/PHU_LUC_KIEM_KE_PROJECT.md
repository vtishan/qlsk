# PHỤ LỤC KIỂM KÊ PROJECT QLSK

Đối chiếu workspace ngày 01/11/2026. Danh mục được chụp trước khi tạo hai tài liệu phân tích; gồm **318 tệp** thuộc nguồn, cấu hình, công cụ, kiểm thử, tài liệu và tài nguyên. Báo cáo chính: [BAO_CAO_PHAN_TICH_HE_THONG.md](BAO_CAO_PHAN_TICH_HE_THONG.md).

SHA-256 dùng để kiểm tra nội dung nguồn không bị thay đổi trong lần lập báo cáo; không đưa nội dung secret vào phụ lục. Hash không phải bằng chứng chức năng đã được chạy thử.

## Phạm vi và cách đọc

- Nguồn/config/test/tool/docs: đọc nội dung; JSON catalog được parse đủ 100 dòng/10 nhóm và so sánh khớp generator mà không chạy lệnh ghi file; SVG được đọc nguồn.
- DOCX cũ: trích xuất văn bản trong word/document.xml để đối chiếu, không coi kết quả lịch sử là kiểm thử hiện tại.
- JPG: giải mã 120/120 tệp, kiểm tra kích thước; không coi ảnh là mã thực thi. Kích thước gồm 100 ảnh 1200×675, 10 ảnh 960×540 và 10 ảnh nguồn rộng 1400 pixel với chiều cao khác nhau.
- Designer/snapshot: metadata EF được đối chiếu cấu hình/model và khác biệt migration, không tính là nghiệp vụ chạy mỗi request.
- Hai tệp tai_lieu_goc.txt và tai_lieu_trien_khai_du_an_ngan_gon.txt có cùng SHA-256; nội dung trùng được đối chiếu một lần.

## Các thư mục không phải nguồn runtime do nhóm viết

| Thư mục tại gốc | Số tệp lúc kiểm kê | Dung lượng bytes | Vai trò |
|---|---:|---:|---|
| artifacts | 2.577 | 308.808.678 | Bản build/publish, dependency Node, ảnh/kết quả kiểm tra cũ; không dùng bản sao để kết luận trạng thái nguồn |
| bin | 484 | 279.093.042 | Assembly/dependency/font/native library đầu ra build |
| obj | 346 | 6.883.896 | Mã trung gian, cache, assets nén, metadata build |

Các số lượng đầu ra có thể thay đổi sau build. Toàn bộ đường dẫn chứa .git, bin, obj, artifacts, publish, node_modules, __pycache__ được loại khỏi danh mục nguồn ở dưới. Không diễn giải từng DLL/cache/vendor như source nghiệp vụ.

## Phân loại

| Định dạng | Số tệp |
|---|---:|
| `(không đuôi)` | 4 |
| `.cjs` | 1 |
| `.cs` | 109 |
| `.csproj` | 4 |
| `.css` | 4 |
| `.docx` | 1 |
| `.html` | 35 |
| `.jpg` | 120 |
| `.js` | 12 |
| `.json` | 5 |
| `.md` | 8 |
| `.ps1` | 1 |
| `.py` | 4 |
| `.sql` | 1 |
| `.svg` | 5 |
| `.txt` | 4 |

## Danh mục 318 tệp

| STT | Tệp (từ gốc project) | Bytes | Cách khảo sát | SHA-256 |
|---:|---|---:|---|---|
| 1 | [.dockerignore](../.dockerignore) | 168 | Đọc nội dung nguồn/cấu hình/tài liệu | `682ef2532be6c7faedfb157e4c4fd334ef6520f2211fbddc07f28881126d2c1b` |
| 2 | [.gitattributes](../.gitattributes) | 26 | Đọc nội dung nguồn/cấu hình/tài liệu | `64f08cd1727f57cff12384cbd03637a20b832dccfa031d3c890e5a18eae5daca` |
| 3 | [.gitignore](../.gitignore) | 500 | Đọc nội dung nguồn/cấu hình/tài liệu | `082a65ae1bd51e09f511ca0d34bcb27d9b5029c48e8842d4a8944039f9d3a5d3` |
| 4 | [appsettings.Development.json](../appsettings.Development.json) | 343 | Đọc nội dung nguồn/cấu hình/tài liệu | `0b1c5599657d7f742743460b2862bc475f9ebbffc48c5bccb9cece780e1ed156` |
| 5 | [appsettings.json](../appsettings.json) | 736 | Đọc nội dung nguồn/cấu hình/tài liệu | `b73f40932cd5d382bd5b809179d2354cf217572349a34f753f90bef103fc9517` |
| 6 | [appsettings.Production.json](../appsettings.Production.json) | 77 | Đọc nội dung nguồn/cấu hình/tài liệu | `7ef3c56bfcf2ea6b6ac7448bc82d55ec4ede4207c6e7d640ae6200f6b32b2b08` |
| 7 | [Common/Auth/ActiveUserFilter.cs](../Common/Auth/ActiveUserFilter.cs) | 1517 | Đọc nội dung nguồn/cấu hình/tài liệu | `f653452967480dd265532606bfd06ea08072eb319dc36305ff930b3d56d1a1d1` |
| 8 | [Common/Auth/JwtOptions.cs](../Common/Auth/JwtOptions.cs) | 255 | Đọc nội dung nguồn/cấu hình/tài liệu | `fc69889130c8b0b4879037e9e7b69df68a6aa1c9d143cfb0df8f3f343cdb7f76` |
| 9 | [Common/Auth/JwtService.cs](../Common/Auth/JwtService.cs) | 1368 | Đọc nội dung nguồn/cấu hình/tài liệu | `f497bf535c5e9a8dc7f05874ac3b70ad6139e8c8a9508217dbbc7bb8ec236a2c` |
| 10 | [Common/Auth/UserClaims.cs](../Common/Auth/UserClaims.cs) | 383 | Đọc nội dung nguồn/cấu hình/tài liệu | `762f479624bcb23f2d4f9463963354089d9cacf17016ffd9de63939be9fb3514` |
| 11 | [Common/Exceptions/ApiException.cs](../Common/Exceptions/ApiException.cs) | 207 | Đọc nội dung nguồn/cấu hình/tài liệu | `65e97715b32956adf60aa4c16334c7bed0de67521b5169c31841c03451555532` |
| 12 | [Common/Exceptions/ApiExceptionMiddleware.cs](../Common/Exceptions/ApiExceptionMiddleware.cs) | 1541 | Đọc nội dung nguồn/cấu hình/tài liệu | `d5d41bbcd7ca92fdabbb665b0d6b161ffdea4711d059e95aa2bd7e77679901bd` |
| 13 | [Common/Models/ApiResponse.cs](../Common/Models/ApiResponse.cs) | 526 | Đọc nội dung nguồn/cấu hình/tài liệu | `6449aac8f92edb4ba6e71b0d7706557eb447a010708caa6f795fae5c3c918c0c` |
| 14 | [Data/ApplicationDbContext.cs](../Data/ApplicationDbContext.cs) | 1310 | Đọc nội dung nguồn/cấu hình/tài liệu | `80aa4ea1bf8ab790108249e8ca61c93e0fe899f0ca47f12c9fa58171ba070fa3` |
| 15 | [Data/Configurations/BanToChucConfig.cs](../Data/Configurations/BanToChucConfig.cs) | 664 | Đọc nội dung nguồn/cấu hình/tài liệu | `daa132936a19ef301d00913f70bacf695382290c4bca000f3e64e9b99d82ded2` |
| 16 | [Data/Configurations/ChiTietDonVeConfig.cs](../Data/Configurations/ChiTietDonVeConfig.cs) | 1248 | Đọc nội dung nguồn/cấu hình/tài liệu | `b1d3028add676c05236aa8976e6344ad5bd136cb3b324520cc848c674dd41aea` |
| 17 | [Data/Configurations/DatDiaDiemConfig.cs](../Data/Configurations/DatDiaDiemConfig.cs) | 1530 | Đọc nội dung nguồn/cấu hình/tài liệu | `9379596a81f95a02d94e48973ea0cb7514b708e85d3018981d95b39955491957` |
| 18 | [Data/Configurations/DiaDiemConfig.cs](../Data/Configurations/DiaDiemConfig.cs) | 1105 | Đọc nội dung nguồn/cấu hình/tài liệu | `0a355a8006abf624087ebe490c267a069218521240468a2bcf013537f1a38da7` |
| 19 | [Data/Configurations/DonVeConfig.cs](../Data/Configurations/DonVeConfig.cs) | 1271 | Đọc nội dung nguồn/cấu hình/tài liệu | `03479ddc713b6ef3447321de403945201db46398b8bee549bc0f815e79a6175c` |
| 20 | [Data/Configurations/LichSuYeuCauXacThucToChucConfig.cs](../Data/Configurations/LichSuYeuCauXacThucToChucConfig.cs) | 917 | Đọc nội dung nguồn/cấu hình/tài liệu | `085f9e086ecd4c7f5f37bae8e3d621c578bf1b05241937d94cd9712e9b0b5bea` |
| 21 | [Data/Configurations/LoaiSuKienConfig.cs](../Data/Configurations/LoaiSuKienConfig.cs) | 521 | Đọc nội dung nguồn/cấu hình/tài liệu | `01e34ad4a77a1851ea4cc2eb253f777d7d3646898f295c0d5f8cf30d2bfe0bd1` |
| 22 | [Data/Configurations/LoaiVeConfig.cs](../Data/Configurations/LoaiVeConfig.cs) | 1400 | Đọc nội dung nguồn/cấu hình/tài liệu | `5d6b58fcd865df9472d5d1535414263112d8e538b209fe80a41efd168a97263c` |
| 23 | [Data/Configurations/SuKienConfig.cs](../Data/Configurations/SuKienConfig.cs) | 1349 | Đọc nội dung nguồn/cấu hình/tài liệu | `046cdd7bd55cbbbe1330a46af86c41681dbe4b2fd18bb7373acd5f197410dbd8` |
| 24 | [Data/Configurations/TaiKhoanConfig.cs](../Data/Configurations/TaiKhoanConfig.cs) | 777 | Đọc nội dung nguồn/cấu hình/tài liệu | `713495b3689204e5b673def5c7ea30ff97b175eaef6aa0502abb87ee2b3e3bd4` |
| 25 | [Data/Configurations/ThanhToanConfig.cs](../Data/Configurations/ThanhToanConfig.cs) | 1183 | Đọc nội dung nguồn/cấu hình/tài liệu | `4851b57696432f115152d5812029c01a9469140edbcdb495357ba5e9439a637b` |
| 26 | [Data/Configurations/VaiTroConfig.cs](../Data/Configurations/VaiTroConfig.cs) | 868 | Đọc nội dung nguồn/cấu hình/tài liệu | `7efb62750025dea5786044ce5b9af570a49cc6c51566f8334d1bc51652a0fd0f` |
| 27 | [Data/Configurations/VeConfig.cs](../Data/Configurations/VeConfig.cs) | 837 | Đọc nội dung nguồn/cấu hình/tài liệu | `4585335f724ccbfa20e2d0886e05688fe4cb40b86fdbaf549d24338fbab89923` |
| 28 | [Data/Configurations/XuatVeConfig.cs](../Data/Configurations/XuatVeConfig.cs) | 631 | Đọc nội dung nguồn/cấu hình/tài liệu | `a982d52c414f891424e7d6121f42748bc8b65191debe3cfab1884de303c373d8` |
| 29 | [Data/Configurations/YeuCauXacThucToChucConfig.cs](../Data/Configurations/YeuCauXacThucToChucConfig.cs) | 903 | Đọc nội dung nguồn/cấu hình/tài liệu | `ba71e70999133408320614cdce5ead2065605820b2fddfb4144ee48e91d99d31` |
| 30 | [Data/schema.sql](../Data/schema.sql) | 9329 | Đọc nội dung nguồn/cấu hình/tài liệu | `a968ccfbb573901b30822548c13114f1b9dd07835676d94a69118edd30b3fef6` |
| 31 | [Dockerfile](../Dockerfile) | 267 | Đọc nội dung nguồn/cấu hình/tài liệu | `68949e2e6b28ad85fed37de9fbc34f03575a1ba44d0131c4bb577cafbdc04a86` |
| 32 | [docs/API.md](../docs/API.md) | 2817 | Đọc nội dung nguồn/cấu hình/tài liệu | `95478d45f34dcfa730c8f25c2ba9d3f3c4d393f734aec6f38bd096f10865aeeb` |
| 33 | [docs/Bao_cao_kiem_thu_va_khac_phuc_QLSK.docx](../docs/Bao_cao_kiem_thu_va_khac_phuc_QLSK.docx) | 44389 | Trích xuất toàn văn XML DOCX | `9f55d927c29103458d83d7212cedb795ce84b060a274c8d9056e9aef3c617624` |
| 34 | [docs/bảng.txt](../docs/bảng.txt) | 29777 | Đọc nội dung nguồn/cấu hình/tài liệu | `5ac11fe52781f5a3f21c68226759b250e0f4d7bf306a9fd8fb16c08fd2440d06` |
| 35 | [docs/chức_năng.txt](../docs/chức_năng.txt) | 21762 | Đọc nội dung nguồn/cấu hình/tài liệu | `937c491445eb198353cfcb297c6c5e3a7800b30733bfbbb28eba3a0594ca87d6` |
| 36 | [docs/PHASES.md](../docs/PHASES.md) | 618 | Đọc nội dung nguồn/cấu hình/tài liệu | `4131174aed4cd67725f3e320c0b5429968b8267e85a03330147f0fb5e61d362f` |
| 37 | [docs/tai_lieu_goc.txt](../docs/tai_lieu_goc.txt) | 29290 | Đọc nội dung nguồn/cấu hình/tài liệu | `8f091ee963fcbc0536a0249205218e5d3bc55c439bbe09ed375da2b4366bf1a5` |
| 38 | [docs/tai_lieu_trien_khai_du_an_ngan_gon.txt](../docs/tai_lieu_trien_khai_du_an_ngan_gon.txt) | 29290 | Đọc nội dung nguồn/cấu hình/tài liệu | `8f091ee963fcbc0536a0249205218e5d3bc55c439bbe09ed375da2b4366bf1a5` |
| 39 | [Domain/Entities/BanToChuc.cs](../Domain/Entities/BanToChuc.cs) | 392 | Đọc nội dung nguồn/cấu hình/tài liệu | `2643425667838bd64b081bb027344f2ff042bc0c4d308e54bcd6fa64642e15b7` |
| 40 | [Domain/Entities/ChiTietDonVe.cs](../Domain/Entities/ChiTietDonVe.cs) | 383 | Đọc nội dung nguồn/cấu hình/tài liệu | `3f474d9dfa313fb1ba1efa1f798f92abca1754e89fe71638cae4a1e03657b8b0` |
| 41 | [Domain/Entities/DatDiaDiem.cs](../Domain/Entities/DatDiaDiem.cs) | 649 | Đọc nội dung nguồn/cấu hình/tài liệu | `2df64433ce7428041a62f319687ffae96510af65880815196cfd7902cfa7e82a` |
| 42 | [Domain/Entities/DiaDiem.cs](../Domain/Entities/DiaDiem.cs) | 453 | Đọc nội dung nguồn/cấu hình/tài liệu | `8fd85aac6574808bfc61eb2be8fd7d615a3a0e5435b725fc189f538c8e2b1684` |
| 43 | [Domain/Entities/DonVe.cs](../Domain/Entities/DonVe.cs) | 780 | Đọc nội dung nguồn/cấu hình/tài liệu | `92bcbc66e9e1fc7e6e016d5d3a0ae209e5aaae6cabe1e00aa781083daff9c103` |
| 44 | [Domain/Entities/LichSuYeuCauXacThucToChuc.cs](../Domain/Entities/LichSuYeuCauXacThucToChuc.cs) | 578 | Đọc nội dung nguồn/cấu hình/tài liệu | `ad82aa1fb5da54151f758838a99e489b460adbb06f6add15c75b0e3ae4f4706d` |
| 45 | [Domain/Entities/LoaiSuKien.cs](../Domain/Entities/LoaiSuKien.cs) | 283 | Đọc nội dung nguồn/cấu hình/tài liệu | `27d9154aa72bebfc4c3bf70e4903b01b27adf741455b0519941ba6f3040b97b5` |
| 46 | [Domain/Entities/LoaiVe.cs](../Domain/Entities/LoaiVe.cs) | 610 | Đọc nội dung nguồn/cấu hình/tài liệu | `db1388e03c69be132eac78b6239017e865084fae3f854de37225e5c332f9c1c2` |
| 47 | [Domain/Entities/SuKien.cs](../Domain/Entities/SuKien.cs) | 905 | Đọc nội dung nguồn/cấu hình/tài liệu | `377ecfa5caa585c043e52cebde8f9c5f4a17eb5809d0563f2e6cf465e7bd302f` |
| 48 | [Domain/Entities/TaiKhoan.cs](../Domain/Entities/TaiKhoan.cs) | 770 | Đọc nội dung nguồn/cấu hình/tài liệu | `8b72a393a06e62a1dc1b798738bbe318a1daa892055981c427699ae14e84d87e` |
| 49 | [Domain/Entities/ThanhToan.cs](../Domain/Entities/ThanhToan.cs) | 658 | Đọc nội dung nguồn/cấu hình/tài liệu | `43b526cf03674d0e60de94789505607ea21635d7ec8dec0eca043a9ea15d9786` |
| 50 | [Domain/Entities/VaiTro.cs](../Domain/Entities/VaiTro.cs) | 236 | Đọc nội dung nguồn/cấu hình/tài liệu | `e08a7e8b1541b389376b050826d67b0dbcaa96d456c88618d50c24024c050281` |
| 51 | [Domain/Entities/Ve.cs](../Domain/Entities/Ve.cs) | 653 | Đọc nội dung nguồn/cấu hình/tài liệu | `368dddf1f855b5ef728ece43faca3bba9868630a264c26245d1da480b024685f` |
| 52 | [Domain/Entities/XuatVe.cs](../Domain/Entities/XuatVe.cs) | 346 | Đọc nội dung nguồn/cấu hình/tài liệu | `a841ed4b8558ff067de9d91baf702198e399cdfd1404deea0c78ec2c858a583c` |
| 53 | [Domain/Entities/YeuCauXacThucToChuc.cs](../Domain/Entities/YeuCauXacThucToChuc.cs) | 620 | Đọc nội dung nguồn/cấu hình/tài liệu | `3144125f0974afa48447565b34fd7c874707a8a1ecb050b3815a8f2f02bc89a6` |
| 54 | [Domain/Enums/TrangThaiDatCho.cs](../Domain/Enums/TrangThaiDatCho.cs) | 109 | Đọc nội dung nguồn/cấu hình/tài liệu | `ee152b72a44fe1e83a8372e67e518229df1acada34e8621bd634596c913a22c0` |
| 55 | [Domain/Enums/TrangThaiDon.cs](../Domain/Enums/TrangThaiDon.cs) | 125 | Đọc nội dung nguồn/cấu hình/tài liệu | `e4af8e281f4a4874ddace79a4d1b5f95f4f4aa78056034d403fc2a1c7801f949` |
| 56 | [Domain/Enums/TrangThaiSuKien.cs](../Domain/Enums/TrangThaiSuKien.cs) | 117 | Đọc nội dung nguồn/cấu hình/tài liệu | `57dec00bedb6de2be5e6a7a879aacbc001f8c2fa09344c16d55598dae9e3634a` |
| 57 | [Domain/Enums/TrangThaiThanhToan.cs](../Domain/Enums/TrangThaiThanhToan.cs) | 102 | Đọc nội dung nguồn/cấu hình/tài liệu | `ee1959037f5d703880ee5dee4d2cda3ca406d8c9dd26d2314667618a34ff0928` |
| 58 | [Domain/Enums/TrangThaiVe.cs](../Domain/Enums/TrangThaiVe.cs) | 98 | Đọc nội dung nguồn/cấu hình/tài liệu | `af1319860cad84026d38317785f9032874bce799cda074344e09bb2a2814f0f3` |
| 59 | [Domain/Enums/TrangThaiYeuCau.cs](../Domain/Enums/TrangThaiYeuCau.cs) | 87 | Đọc nội dung nguồn/cấu hình/tài liệu | `8670fd58626b5321163c4497680707d6709b22b544df9ec69f5ef03da071e061` |
| 60 | [dotnet-tools.json](../dotnet-tools.json) | 194 | Đọc nội dung nguồn/cấu hình/tài liệu | `12c087b50d066776ca305d0de304e3ea661e067d5b1f3168a513685ed75c6c53` |
| 61 | [Features/DiaDiem/DiaDiemController.cs](../Features/DiaDiem/DiaDiemController.cs) | 1984 | Đọc nội dung nguồn/cấu hình/tài liệu | `b1294ed603e0bd55b0f3a157750e1b13ee4409ac13613a308380679a7cd61e82` |
| 62 | [Features/DiaDiem/DiaDiemService.cs](../Features/DiaDiem/DiaDiemService.cs) | 4960 | Đọc nội dung nguồn/cấu hình/tài liệu | `be4809f8d8c0e5856b8076d036d7f41ab0f1d706608080b395d0666632d86287` |
| 63 | [Features/DiaDiem/Dtos.cs](../Features/DiaDiem/Dtos.cs) | 916 | Đọc nội dung nguồn/cấu hình/tài liệu | `e0ba151e44449c1ab49bb8fb761a6a139f289ca07c0e6a0f635446a5e1b87e17` |
| 64 | [Features/DonVe/DonVeController.cs](../Features/DonVe/DonVeController.cs) | 900 | Đọc nội dung nguồn/cấu hình/tài liệu | `a8aaf33e723752b392fd74158970ffb51b36835e51e9c4040caa9304d1ecc56e` |
| 65 | [Features/DonVe/DonVeService.cs](../Features/DonVe/DonVeService.cs) | 5809 | Đọc nội dung nguồn/cấu hình/tài liệu | `8d1146c9cc8cf9888f32023da433b4ec3fd72407c4d07a8a928fd325350b1668` |
| 66 | [Features/DonVe/Dtos.cs](../Features/DonVe/Dtos.cs) | 329 | Đọc nội dung nguồn/cấu hình/tài liệu | `8081383a04b3518ba7b0436c60ef670df753e8d4388b5fa1b4c48929a309322f` |
| 67 | [Features/LoaiVe/Dtos.cs](../Features/LoaiVe/Dtos.cs) | 382 | Đọc nội dung nguồn/cấu hình/tài liệu | `4d9fc79d14fcd3e333f3163f2239b3d6121322c29c81f489f33ce15b97d695dc` |
| 68 | [Features/LoaiVe/LoaiVeController.cs](../Features/LoaiVe/LoaiVeController.cs) | 1606 | Đọc nội dung nguồn/cấu hình/tài liệu | `02b225db10862ba7feddbe1f70b60013f0193029c5f0606fcd4b1fbd5581e6a9` |
| 69 | [Features/LoaiVe/LoaiVeService.cs](../Features/LoaiVe/LoaiVeService.cs) | 4591 | Đọc nội dung nguồn/cấu hình/tài liệu | `35fb80a8d498a237b0f514d5d5491dd0b52684cf093fd8cd263f7a02f1a1c7c7` |
| 70 | [Features/QuanTri/Dtos.cs](../Features/QuanTri/Dtos.cs) | 721 | Đọc nội dung nguồn/cấu hình/tài liệu | `ead4eaf1d88dd8a095fc882ad77ed6afc2aae0564624bae3de90317a5823267a` |
| 71 | [Features/QuanTri/QuanTriController.cs](../Features/QuanTri/QuanTriController.cs) | 1625 | Đọc nội dung nguồn/cấu hình/tài liệu | `6a0b24d5dc4159397f68e0654d39c8caa1dadfcc2bd27185df5e65f320d14230` |
| 72 | [Features/QuanTri/QuanTriService.cs](../Features/QuanTri/QuanTriService.cs) | 7426 | Đọc nội dung nguồn/cấu hình/tài liệu | `97f2231ecc2261ca87f8b01db8dd76998f0d1338dd08e146f050b635cb453eb4` |
| 73 | [Features/SuKien/Dtos.cs](../Features/SuKien/Dtos.cs) | 936 | Đọc nội dung nguồn/cấu hình/tài liệu | `447277f26b5eaa640861605c1fff4dd904d68ad4924ed8cc367bcf5576e07fef` |
| 74 | [Features/SuKien/SuKienController.cs](../Features/SuKien/SuKienController.cs) | 2957 | Đọc nội dung nguồn/cấu hình/tài liệu | `4ad527d16f64fd72d6ff0c6eef5db2ee199325e4dfa8bfaee90741b79736ffa0` |
| 75 | [Features/SuKien/SuKienService.cs](../Features/SuKien/SuKienService.cs) | 7649 | Đọc nội dung nguồn/cấu hình/tài liệu | `b5b55badb55d4d4a7108e4135b2ebc8eaa00c6e74a3db654af8dad294b3bea10` |
| 76 | [Features/TaiKhoan/AuthController.cs](../Features/TaiKhoan/AuthController.cs) | 1006 | Đọc nội dung nguồn/cấu hình/tài liệu | `d7bbe256f88f6fdd6e10233e521870c7574d752175459e923ba6235a60564139` |
| 77 | [Features/TaiKhoan/Dtos.cs](../Features/TaiKhoan/Dtos.cs) | 1046 | Đọc nội dung nguồn/cấu hình/tài liệu | `533b135bfa1fd4c28880b29bb9edf02ee70bde38e94a42059a90deb5e671f13c` |
| 78 | [Features/TaiKhoan/TaiKhoanController.cs](../Features/TaiKhoan/TaiKhoanController.cs) | 1578 | Đọc nội dung nguồn/cấu hình/tài liệu | `d3caf27842597a523642b2432754df3ab9e4621171c1a058c8119352e2ee5b5d` |
| 79 | [Features/TaiKhoan/TaiKhoanService.cs](../Features/TaiKhoan/TaiKhoanService.cs) | 4489 | Đọc nội dung nguồn/cấu hình/tài liệu | `3cec11309dead90c67258ecead6cf69840e9cbe2e9a79f85568589788f61f71c` |
| 80 | [Features/ThanhToan/Dtos.cs](../Features/ThanhToan/Dtos.cs) | 347 | Đọc nội dung nguồn/cấu hình/tài liệu | `7af97c30c30a6a2a9dfa08ac6f85037446eb9c3885fa37aae5b57e78706f6776` |
| 81 | [Features/ThanhToan/ThanhToanController.cs](../Features/ThanhToan/ThanhToanController.cs) | 926 | Đọc nội dung nguồn/cấu hình/tài liệu | `591a3afab73773c784f471100668bcea1beb4566746cdb7208013b91a071ab12` |
| 82 | [Features/ThanhToan/ThanhToanService.cs](../Features/ThanhToan/ThanhToanService.cs) | 9878 | Đọc nội dung nguồn/cấu hình/tài liệu | `8f307603bb92ebd40bab5ad34ab652ae396ae73e3976d33411371aedfe3dc0fd` |
| 83 | [Features/ThongKe/ThongKeController.cs](../Features/ThongKe/ThongKeController.cs) | 516 | Đọc nội dung nguồn/cấu hình/tài liệu | `3f36e4629f2c3d415bc6fcfe4f07a23100c7781a1283482b690ec039c68870e9` |
| 84 | [Features/ThongKe/ThongKeService.cs](../Features/ThongKe/ThongKeService.cs) | 1962 | Đọc nội dung nguồn/cấu hình/tài liệu | `a36e7330ac868393d8414596abd339a7ab2359f684f7603c817cdc585e3a049d` |
| 85 | [Features/Ve/VeController.cs](../Features/Ve/VeController.cs) | 1145 | Đọc nội dung nguồn/cấu hình/tài liệu | `9d634dbea98655d25017c2dc5584dd58e07d587255dd799e4a195b03127c3a80` |
| 86 | [Features/Ve/VeService.cs](../Features/Ve/VeService.cs) | 2269 | Đọc nội dung nguồn/cấu hình/tài liệu | `c506257c1081e2385f4bb0f9ba6cef023e1d612c63a4111be5702d9e9a0d2924` |
| 87 | [Features/XacThucToChuc/Dtos.cs](../Features/XacThucToChuc/Dtos.cs) | 364 | Đọc nội dung nguồn/cấu hình/tài liệu | `958702773e161d1cec48b6c2b7c2bda9c0b94f15fd1bf86b6ed8fb3963ad8ae0` |
| 88 | [Features/XacThucToChuc/XacThucController.cs](../Features/XacThucToChuc/XacThucController.cs) | 1252 | Đọc nội dung nguồn/cấu hình/tài liệu | `9e801a7780ac84378198e3891380cbfce7f82493107bf917f325241001f1ee78` |
| 89 | [Features/XacThucToChuc/XacThucService.cs](../Features/XacThucToChuc/XacThucService.cs) | 3316 | Đọc nội dung nguồn/cấu hình/tài liệu | `c11dd049ccc4edac2ff5b05e93b8b253e1cc420514ae0f7e5f6b5b75d28d7e2d` |
| 90 | [Features/XuatVe/XuatVeController.cs](../Features/XuatVe/XuatVeController.cs) | 775 | Đọc nội dung nguồn/cấu hình/tài liệu | `ca319c203500f9b0a09d1a8e80513fb4f0a824285159620ebcf0f7f6e804f60d` |
| 91 | [Features/XuatVe/XuatVeService.cs](../Features/XuatVe/XuatVeService.cs) | 2049 | Đọc nội dung nguồn/cấu hình/tài liệu | `df8e7530d834753e397b2632ad25cd2d916f5625307c448e5623d9ebf20e2739` |
| 92 | [Infrastructure/Email/EmailOptions.cs](../Infrastructure/Email/EmailOptions.cs) | 332 | Đọc nội dung nguồn/cấu hình/tài liệu | `7dc6c70e4d8cb0aba04f8cc3302acb38161bcb07cdb34f3885a236ea16963aae` |
| 93 | [Infrastructure/Email/EmailService.cs](../Infrastructure/Email/EmailService.cs) | 1346 | Đọc nội dung nguồn/cấu hình/tài liệu | `cc91cd4a2c9cad35ba849105f612edc09176bbb1a42f6a9579c531322fed4c80` |
| 94 | [Infrastructure/EventStateWorker.cs](../Infrastructure/EventStateWorker.cs) | 2047 | Đọc nội dung nguồn/cấu hình/tài liệu | `63a5df989dd3db2a851c2cd1b02e9c765a29793d1a88a516d9894b24bde5a2b5` |
| 95 | [Infrastructure/ExpiredOrderWorker.cs](../Infrastructure/ExpiredOrderWorker.cs) | 1165 | Đọc nội dung nguồn/cấu hình/tài liệu | `f940e5fd954c2eb0968aae69ce6e328ee56cbaad93e0486fc9b2173151046153` |
| 96 | [Infrastructure/File/VePdfService.cs](../Infrastructure/File/VePdfService.cs) | 1426 | Đọc nội dung nguồn/cấu hình/tài liệu | `9e16566b01207fef680aa0bb2b5d83d93976a7011130a56d5a0d60fe2923c294` |
| 97 | [Infrastructure/PayOS/PayOsModels.cs](../Infrastructure/PayOS/PayOsModels.cs) | 504 | Đọc nội dung nguồn/cấu hình/tài liệu | `14c27c5bcf35608517e9e898470d7b0776ecfd6e291af387620361d32eb73ced` |
| 98 | [Infrastructure/PayOS/PayOsOptions.cs](../Infrastructure/PayOS/PayOsOptions.cs) | 407 | Đọc nội dung nguồn/cấu hình/tài liệu | `d96751ee4900b9b8dffc196f00d5ce1d1c2c85e480a13c1c0b2a0cb096d341be` |
| 99 | [Infrastructure/PayOS/PayOsService.cs](../Infrastructure/PayOS/PayOsService.cs) | 8094 | Đọc nội dung nguồn/cấu hình/tài liệu | `b85b6096b0c37a96e26356e536104ef341abc77558a65b141e91e004e20cc72b` |
| 100 | [Infrastructure/QR/QrService.cs](../Infrastructure/QR/QrService.cs) | 373 | Đọc nội dung nguồn/cấu hình/tài liệu | `8b3fa0bf5a2aead7837521da9931d580cb494287e41c2684e998beead5c3d4bb` |
| 101 | [Migrations/20261027130000_InitialCreate.cs](../Migrations/20261027130000_InitialCreate.cs) | 7313 | Đọc nội dung nguồn/cấu hình/tài liệu | `66a84ee0671e58cc050bd1f723319f441ca72f2425751daa7bcaf2fe040c23f0` |
| 102 | [Migrations/20261027150246_AddUniquePaymentLinkIndex.cs](../Migrations/20261027150246_AddUniquePaymentLinkIndex.cs) | 835 | Đọc nội dung nguồn/cấu hình/tài liệu | `6cfb64f976a617786bf457ba985dadb9d42ac54051938915bc731df82c382cdc` |
| 103 | [Migrations/20261027150246_AddUniquePaymentLinkIndex.Designer.cs](../Migrations/20261027150246_AddUniquePaymentLinkIndex.Designer.cs) | 36478 | Đọc/đối chiếu metadata EF và diff | `bb448887f4d43e936677dcb135494ff6621a54c0aeb9001154175509b8b10c39` |
| 104 | [Migrations/20261031152901_AddEventImagePath.cs](../Migrations/20261031152901_AddEventImagePath.cs) | 783 | Đọc nội dung nguồn/cấu hình/tài liệu | `ca04a1ff89478f594780abf05fe2bc517719a1261f0f176c7b0987384c1f4281` |
| 105 | [Migrations/20261031152901_AddEventImagePath.Designer.cs](../Migrations/20261031152901_AddEventImagePath.Designer.cs) | 36617 | Đọc/đối chiếu metadata EF và diff | `74921e8b028ea4b89272944ad8b6e4cb5fbb12420954dd09dc2c5157eeeef319` |
| 106 | [Migrations/20261031170820_AddVenueImagePath.cs](../Migrations/20261031170820_AddVenueImagePath.cs) | 785 | Đọc nội dung nguồn/cấu hình/tài liệu | `34a471356e1aec13ebb6e69d26a314a807d4edac3d2865888162724a873a9f90` |
| 107 | [Migrations/20261031170820_AddVenueImagePath.Designer.cs](../Migrations/20261031170820_AddVenueImagePath.Designer.cs) | 36772 | Đọc/đối chiếu metadata EF và diff | `df47fe96489dc26e74f06f0bb33aff7342899458b018a23518ff434db6142964` |
| 108 | [Migrations/ApplicationDbContextModelSnapshot.cs](../Migrations/ApplicationDbContextModelSnapshot.cs) | 36668 | Đọc/đối chiếu metadata EF và diff | `c87b68118ea8837a32808133a6089c663b74283b38f41142e0c44eaf5d032043` |
| 109 | [Program.cs](../Program.cs) | 6227 | Đọc nội dung nguồn/cấu hình/tài liệu | `052496d22e51e428460a6cfed18a0b7ab43fc87a3658f13aa7fd905a2df408d0` |
| 110 | [QLSK.csproj](../QLSK.csproj) | 1521 | Đọc nội dung nguồn/cấu hình/tài liệu | `a656f819fe7120bb7479e0a7af8479ec331e1695053fb50ae02fc31846542e55` |
| 111 | [README.md](../README.md) | 17047 | Đọc nội dung nguồn/cấu hình/tài liệu | `47c86a2a3a39833033e2cddd724778fdd8efe5745c1e6f5c3b8d3400b8be0aef` |
| 112 | [scripts/generate-venue-images.py](../scripts/generate-venue-images.py) | 4016 | Đọc nội dung nguồn/cấu hình/tài liệu | `08cb1fde5c1da53108282c214e640ac4827cd82d1a28291e42b7877fd7f5b2ac` |
| 113 | [scripts/Set-AdminCredential.ps1](../scripts/Set-AdminCredential.ps1) | 1593 | Đọc nội dung nguồn/cấu hình/tài liệu | `f3a6197fe5b27ec0e1a6f07f627b48c2f4035aee4e8a08456857fc001e6d6baf` |
| 114 | [scripts/version-frontend-assets.py](../scripts/version-frontend-assets.py) | 1705 | Đọc nội dung nguồn/cấu hình/tài liệu | `52fc65bee786e3c623ff8bed3181dcdc26389036f85bced1f3c009c527a53d60` |
| 115 | [tests/frontend-data.test.cjs](../tests/frontend-data.test.cjs) | 4128 | Đọc nội dung nguồn/cấu hình/tài liệu | `8db840e9504023e6b4cf419a2cf5623244c957478f4ff24c8959e954e6f289cb` |
| 116 | [tests/QLSK.LocalData/catalog/events.json](../tests/QLSK.LocalData/catalog/events.json) | 48560 | Parse 100 record, khớp generator | `b90d2702c40248f8bd9ddf24104ea4d5701574101a51ffcde48c7b520361ba7e` |
| 117 | [tests/QLSK.LocalData/LocalEventNormalizer.cs](../tests/QLSK.LocalData/LocalEventNormalizer.cs) | 6457 | Đọc nội dung nguồn/cấu hình/tài liệu | `e61d09f6f0062a48026e4f70355c78cf4371ec0b01d6b13f00953f84bd4c84bb` |
| 118 | [tests/QLSK.LocalData/LocalEventSeeder.cs](../tests/QLSK.LocalData/LocalEventSeeder.cs) | 8766 | Đọc nội dung nguồn/cấu hình/tài liệu | `b39c31ee11540198cf959445c90973c229f4cbca3179c69938366ed1b644f9b5` |
| 119 | [tests/QLSK.LocalData/LocalSqlGuard.cs](../tests/QLSK.LocalData/LocalSqlGuard.cs) | 1568 | Đọc nội dung nguồn/cấu hình/tài liệu | `5e24968351d61669653ccbed7945fcd7e2fe8533c829b0630856892dd1ff767d` |
| 120 | [tests/QLSK.LocalData/Program.cs](../tests/QLSK.LocalData/Program.cs) | 4524 | Đọc nội dung nguồn/cấu hình/tài liệu | `7870b754ec2d3264e72d9816e92cd93107cbf3b9dbdee0b5a46c051bb3d67c8b` |
| 121 | [tests/QLSK.LocalData/QLSK.LocalData.csproj](../tests/QLSK.LocalData/QLSK.LocalData.csproj) | 482 | Đọc nội dung nguồn/cấu hình/tài liệu | `b34f53653ca789a956da685efce31afce18a9c829d716c7a890182e278317afe` |
| 122 | [tests/QLSK.LocalData/README.md](../tests/QLSK.LocalData/README.md) | 2707 | Đọc nội dung nguồn/cấu hình/tài liệu | `155ce61ab45bd52b1a912c5a17eae3d09b4557e951e344f67a591cae4de45a41` |
| 123 | [tests/QLSK.LocalData/scripts/create-event-catalog.py](../tests/QLSK.LocalData/scripts/create-event-catalog.py) | 6511 | Đọc nội dung nguồn/cấu hình/tài liệu | `df0d10268b69562823238bdaadb9a9bb227fdcd94165a0604f4e590099991a29` |
| 124 | [tests/QLSK.LocalData/scripts/generate-event-images.py](../tests/QLSK.LocalData/scripts/generate-event-images.py) | 7722 | Đọc nội dung nguồn/cấu hình/tài liệu | `60f6034a1d1e87b49862b7c1d5ac27a1cc5caa56c952d32b419591369a7b514e` |
| 125 | [tests/QLSK.Tests/ApiEndpointTests.cs](../tests/QLSK.Tests/ApiEndpointTests.cs) | 23896 | Đọc nội dung nguồn/cấu hình/tài liệu | `cfe2a3077301a6118a368f4e7bac797d34e656f68bbcf26ed9a1b9e4e6f85e9e` |
| 126 | [tests/QLSK.Tests/ApiFactory.cs](../tests/QLSK.Tests/ApiFactory.cs) | 7738 | Đọc nội dung nguồn/cấu hình/tài liệu | `217976fb6622ef207c6e4a5a6c911c52233e1a1c65d38b2185fbc732023a6bac` |
| 127 | [tests/QLSK.Tests/ApiRegressionTests.cs](../tests/QLSK.Tests/ApiRegressionTests.cs) | 12827 | Đọc nội dung nguồn/cấu hình/tài liệu | `16b9e3732be4daf31f74695a02eed93376d77938883b179eeb8603385ca5c634` |
| 128 | [tests/QLSK.Tests/ApiTestData.cs](../tests/QLSK.Tests/ApiTestData.cs) | 6469 | Đọc nội dung nguồn/cấu hình/tài liệu | `2180ab04629c22a45a10455a082d3379731c207570a21b0e4587594a4adac153` |
| 129 | [tests/QLSK.Tests/AuthTests.cs](../tests/QLSK.Tests/AuthTests.cs) | 1935 | Đọc nội dung nguồn/cấu hình/tài liệu | `5360352d501e0972d28b037a037e98f06d5e31d92cf79f0ab8da914786fe98a7` |
| 130 | [tests/QLSK.Tests/DeploymentSetupTests.cs](../tests/QLSK.Tests/DeploymentSetupTests.cs) | 4573 | Đọc nội dung nguồn/cấu hình/tài liệu | `3e14397996a503f9dad0e46cefa0dd501de4bf66b7a3c5a973e04300fbebad89` |
| 131 | [tests/QLSK.Tests/LocalEventDataTests.cs](../tests/QLSK.Tests/LocalEventDataTests.cs) | 12690 | Đọc nội dung nguồn/cấu hình/tài liệu | `0d1d0c70beb594a52c33be51cddcfe29f3592a6ab365b592a66aae1af193e2de` |
| 132 | [tests/QLSK.Tests/QLSK.Tests.csproj](../tests/QLSK.Tests/QLSK.Tests.csproj) | 854 | Đọc nội dung nguồn/cấu hình/tài liệu | `9e69409c4e479434a5a1c81e32610ef5313a94d8a5bbd41119ca3212f757e754` |
| 133 | [tests/QLSK.Tests/RealDataTests.cs](../tests/QLSK.Tests/RealDataTests.cs) | 9162 | Đọc nội dung nguồn/cấu hình/tài liệu | `6b830c79ccaf571d9d6de0a096e5ad5a2d9ca077007f91fc4dda8f5021a10428` |
| 134 | [tests/QLSK.Tests/RuleTests.cs](../tests/QLSK.Tests/RuleTests.cs) | 2830 | Đọc nội dung nguồn/cấu hình/tài liệu | `f3b40a10b8ea2e30d58714962958e2b8b35a24088fb5728e0d5c0f274e6dff9b` |
| 135 | [tests/TEST_PLAN.md](../tests/TEST_PLAN.md) | 894 | Đọc nội dung nguồn/cấu hình/tài liệu | `bef73bf4dbe5688a71c0b004b1d299bfcc4e662c015158421c020f857768c360` |
| 136 | [tools/QLSK.Setup/AdminProvisioner.cs](../tools/QLSK.Setup/AdminProvisioner.cs) | 3996 | Đọc nội dung nguồn/cấu hình/tài liệu | `7b3dd7966e87d3313a5146f763c18af82c787f9426daa5f7ca103c349944ad79` |
| 137 | [tools/QLSK.Setup/LegacyDataMigrator.cs](../tools/QLSK.Setup/LegacyDataMigrator.cs) | 3302 | Đọc nội dung nguồn/cấu hình/tài liệu | `9fb91670394e128d0436c7d55de61cc7bdb50516dcd87c182b801e64d87d2c5e` |
| 138 | [tools/QLSK.Setup/Program.cs](../tools/QLSK.Setup/Program.cs) | 5196 | Đọc nội dung nguồn/cấu hình/tài liệu | `eea22fd4fa3fcb204d09bdf71234d1419618bd7f7a7f37ec14172ad6745e450c` |
| 139 | [tools/QLSK.Setup/QLSK.Setup.csproj](../tools/QLSK.Setup/QLSK.Setup.csproj) | 355 | Đọc nội dung nguồn/cấu hình/tài liệu | `4353bc2d95548fab076844124b5757c32ee00ab4fc20d2d4d7dfb139b0b37ddc` |
| 140 | [tools/QLSK.Setup/README.md](../tools/QLSK.Setup/README.md) | 2867 | Đọc nội dung nguồn/cấu hình/tài liệu | `901dc02ab3001a4805b9e1dfc6a25fc0f1ee2e90088ed1226c18cba898467ca1` |
| 141 | [wwwroot/assets/icons/favicon.svg](../wwwroot/assets/icons/favicon.svg) | 236 | Đọc XML/SVG | `e104e853ff548af47fdbf59e62b83312f85df16736a42fa5b17fe3917c76dec4` |
| 142 | [wwwroot/assets/icons/logo.svg](../wwwroot/assets/icons/logo.svg) | 236 | Đọc XML/SVG | `e104e853ff548af47fdbf59e62b83312f85df16736a42fa5b17fe3917c76dec4` |
| 143 | [wwwroot/assets/images/events/event-001.jpg](../wwwroot/assets/images/events/event-001.jpg) | 159565 | Nhị phân: giải mã ảnh/kích thước | `94e6bffcfb785d0c0bec9536f7ac3efd2ed0a076701314a0e3783f2448757b41` |
| 144 | [wwwroot/assets/images/events/event-002.jpg](../wwwroot/assets/images/events/event-002.jpg) | 156958 | Nhị phân: giải mã ảnh/kích thước | `270ade5aa69ad9fffecbb242894e502b043035508a3a2055a1e888cfbdaafd83` |
| 145 | [wwwroot/assets/images/events/event-003.jpg](../wwwroot/assets/images/events/event-003.jpg) | 156043 | Nhị phân: giải mã ảnh/kích thước | `346b4387785d1612f6e363208a5b14e941be64b58c0ad4f77b0d1daee73be978` |
| 146 | [wwwroot/assets/images/events/event-004.jpg](../wwwroot/assets/images/events/event-004.jpg) | 157123 | Nhị phân: giải mã ảnh/kích thước | `a3c8a229c499e1a03e4cd15177218c61a80e62b2d07f61f883b9e98f6c58bdcb` |
| 147 | [wwwroot/assets/images/events/event-005.jpg](../wwwroot/assets/images/events/event-005.jpg) | 156929 | Nhị phân: giải mã ảnh/kích thước | `df3e461daa97e55723ade85711916120b735dd83ec4a2fc5c302a39d48acf7ed` |
| 148 | [wwwroot/assets/images/events/event-006.jpg](../wwwroot/assets/images/events/event-006.jpg) | 158458 | Nhị phân: giải mã ảnh/kích thước | `ac8d1b4d400e7ecb31c17fd2d76ee492387863dcc8e3aaa96ea46f3232218447` |
| 149 | [wwwroot/assets/images/events/event-007.jpg](../wwwroot/assets/images/events/event-007.jpg) | 157930 | Nhị phân: giải mã ảnh/kích thước | `8f24fcf88e5fc6060131af52b4c92da089d52ee30174bf944d772d3e9a76e03c` |
| 150 | [wwwroot/assets/images/events/event-008.jpg](../wwwroot/assets/images/events/event-008.jpg) | 154730 | Nhị phân: giải mã ảnh/kích thước | `bd878abbae7bb146ea67d60b2b65c6d390c59a024c3f36a3485ab2ea541ade98` |
| 151 | [wwwroot/assets/images/events/event-009.jpg](../wwwroot/assets/images/events/event-009.jpg) | 156835 | Nhị phân: giải mã ảnh/kích thước | `318f129dfdd9b2cf18e08147a5450b58ed5336cbe1335d58b1ac8b781a8a5870` |
| 152 | [wwwroot/assets/images/events/event-010.jpg](../wwwroot/assets/images/events/event-010.jpg) | 157145 | Nhị phân: giải mã ảnh/kích thước | `e27cde3f98193136192ab0d0b27ca260e355891ceb93bcbb60e5024b5d54d042` |
| 153 | [wwwroot/assets/images/events/event-011.jpg](../wwwroot/assets/images/events/event-011.jpg) | 126469 | Nhị phân: giải mã ảnh/kích thước | `db8e9bb6a125f614dd5dfd9c2fd53ecec86b8412906ba4ff0968a0bb6638afad` |
| 154 | [wwwroot/assets/images/events/event-012.jpg](../wwwroot/assets/images/events/event-012.jpg) | 126539 | Nhị phân: giải mã ảnh/kích thước | `f71b712d64bd63a4a1f73a62a296906229b6dbef48ed2be5968dad12b488744d` |
| 155 | [wwwroot/assets/images/events/event-013.jpg](../wwwroot/assets/images/events/event-013.jpg) | 127233 | Nhị phân: giải mã ảnh/kích thước | `c6c8682b1c9c241e83c706e53be8d4f0179e32dea6fff90b787f6eb0f1c9859a` |
| 156 | [wwwroot/assets/images/events/event-014.jpg](../wwwroot/assets/images/events/event-014.jpg) | 128704 | Nhị phân: giải mã ảnh/kích thước | `aaa49efffe1599ce9227e751bab341da51d5cf1728bd9053e95bb9974567a703` |
| 157 | [wwwroot/assets/images/events/event-015.jpg](../wwwroot/assets/images/events/event-015.jpg) | 127262 | Nhị phân: giải mã ảnh/kích thước | `bbe22cc186fc967c5daa99321bd86219deb4d3c2afe6e21c36bd87c28d4707cc` |
| 158 | [wwwroot/assets/images/events/event-016.jpg](../wwwroot/assets/images/events/event-016.jpg) | 128389 | Nhị phân: giải mã ảnh/kích thước | `389907da7294ea948e578a3b1dd438d56426fce6f711e045c03e4cf95197688d` |
| 159 | [wwwroot/assets/images/events/event-017.jpg](../wwwroot/assets/images/events/event-017.jpg) | 126963 | Nhị phân: giải mã ảnh/kích thước | `df96c07baeeb7b74dba1b7467e878bdec3823fe4bac522ddc68d30e6de8f41b0` |
| 160 | [wwwroot/assets/images/events/event-018.jpg](../wwwroot/assets/images/events/event-018.jpg) | 127339 | Nhị phân: giải mã ảnh/kích thước | `e910b33fa57c46d512bd65b2b682ca4654d22c3323574319a09ae605f982b0dd` |
| 161 | [wwwroot/assets/images/events/event-019.jpg](../wwwroot/assets/images/events/event-019.jpg) | 127776 | Nhị phân: giải mã ảnh/kích thước | `6fd0f557042cb7114ed01e95579cf208facf7fdf109801418b8599c89a22af8f` |
| 162 | [wwwroot/assets/images/events/event-020.jpg](../wwwroot/assets/images/events/event-020.jpg) | 126121 | Nhị phân: giải mã ảnh/kích thước | `1867264a3c1beab8398a7d5f23c729a71452ae34574a53be678088a876fed1e9` |
| 163 | [wwwroot/assets/images/events/event-021.jpg](../wwwroot/assets/images/events/event-021.jpg) | 105203 | Nhị phân: giải mã ảnh/kích thước | `c7ed5e843ff994c4278592d8df64f03df9055d171bd459a5e5fa7d070d88829f` |
| 164 | [wwwroot/assets/images/events/event-022.jpg](../wwwroot/assets/images/events/event-022.jpg) | 107598 | Nhị phân: giải mã ảnh/kích thước | `3968d1a8eb9d61461db64aa5a7fec77ccbcbd8b9e33bd1fe577f08f1903ed397` |
| 165 | [wwwroot/assets/images/events/event-023.jpg](../wwwroot/assets/images/events/event-023.jpg) | 106608 | Nhị phân: giải mã ảnh/kích thước | `436cb3cfffa8aa3e9d07289abcd94c07c346a48b5e537c69134dcb92d25d7768` |
| 166 | [wwwroot/assets/images/events/event-024.jpg](../wwwroot/assets/images/events/event-024.jpg) | 106662 | Nhị phân: giải mã ảnh/kích thước | `9648a14db82e94fa78a541a9540d8c1a5e240adbda3785c793d04aed9acf8468` |
| 167 | [wwwroot/assets/images/events/event-025.jpg](../wwwroot/assets/images/events/event-025.jpg) | 105872 | Nhị phân: giải mã ảnh/kích thước | `7c130a7c1775f79a1b41f3e0f739053b4cea411ce6e136d4bf0c7c86180a373b` |
| 168 | [wwwroot/assets/images/events/event-026.jpg](../wwwroot/assets/images/events/event-026.jpg) | 106561 | Nhị phân: giải mã ảnh/kích thước | `bd2ed152a1df64c625c9ca6cb9b43ca47a438cc6bf3d68a8bb639657daeea8f4` |
| 169 | [wwwroot/assets/images/events/event-027.jpg](../wwwroot/assets/images/events/event-027.jpg) | 107880 | Nhị phân: giải mã ảnh/kích thước | `e6c0d5563f4d6bbd3c2d68418716d84f3aabc1c0fac553b46868b1dfabc2e767` |
| 170 | [wwwroot/assets/images/events/event-028.jpg](../wwwroot/assets/images/events/event-028.jpg) | 105378 | Nhị phân: giải mã ảnh/kích thước | `86c7cae8f4ff5aae8f82927094290b0d1552be24690f788606c092dfbe0e5bb8` |
| 171 | [wwwroot/assets/images/events/event-029.jpg](../wwwroot/assets/images/events/event-029.jpg) | 106973 | Nhị phân: giải mã ảnh/kích thước | `e67d5140061c12444503f8cd8e4d8d6476a0f2a3a24d9320b732ebcc54b55161` |
| 172 | [wwwroot/assets/images/events/event-030.jpg](../wwwroot/assets/images/events/event-030.jpg) | 105312 | Nhị phân: giải mã ảnh/kích thước | `7b7d552474c446d7a66299a5e3f2869240f884906a21d864372cd56ad77f8bf3` |
| 173 | [wwwroot/assets/images/events/event-031.jpg](../wwwroot/assets/images/events/event-031.jpg) | 152142 | Nhị phân: giải mã ảnh/kích thước | `bccced9646bb68af2afcc21cd925141fb98691cd4194110a6c5f375e8b0db2e1` |
| 174 | [wwwroot/assets/images/events/event-032.jpg](../wwwroot/assets/images/events/event-032.jpg) | 153085 | Nhị phân: giải mã ảnh/kích thước | `178c83b78ff5929f228208ce725b90b88f15343179ab2d9eff0732c45a32d8bf` |
| 175 | [wwwroot/assets/images/events/event-033.jpg](../wwwroot/assets/images/events/event-033.jpg) | 155117 | Nhị phân: giải mã ảnh/kích thước | `0e35b443f704dad4f1e62b710cac638694464683b0e4618f1f0ec7eb83ebfe2a` |
| 176 | [wwwroot/assets/images/events/event-034.jpg](../wwwroot/assets/images/events/event-034.jpg) | 151708 | Nhị phân: giải mã ảnh/kích thước | `5c280db1168c7e23ea1fbf1a50da6354f8d59cb1bfbf241c8a6fe537e7a53f5f` |
| 177 | [wwwroot/assets/images/events/event-035.jpg](../wwwroot/assets/images/events/event-035.jpg) | 153267 | Nhị phân: giải mã ảnh/kích thước | `dea9527fffb0fdc55335a99a89cf6a02d5778d3ac2a50d55f77e6fb15f316068` |
| 178 | [wwwroot/assets/images/events/event-036.jpg](../wwwroot/assets/images/events/event-036.jpg) | 153265 | Nhị phân: giải mã ảnh/kích thước | `c2e5ed7753d9f87a7ae026a21eac5dabd75c603118e523f552249e1f30429784` |
| 179 | [wwwroot/assets/images/events/event-037.jpg](../wwwroot/assets/images/events/event-037.jpg) | 154920 | Nhị phân: giải mã ảnh/kích thước | `68578519abb2211238635176572898a16289cfaa7b2e6e2b7698168cc909f5be` |
| 180 | [wwwroot/assets/images/events/event-038.jpg](../wwwroot/assets/images/events/event-038.jpg) | 152050 | Nhị phân: giải mã ảnh/kích thước | `968f5dbe5ac21031b6d40ea1f2c8e45f791166bfb45a0810d42945e268c4bc63` |
| 181 | [wwwroot/assets/images/events/event-039.jpg](../wwwroot/assets/images/events/event-039.jpg) | 152274 | Nhị phân: giải mã ảnh/kích thước | `bf7638b8326d556a38506d9f6d75d5fad3995b23133d35a047a6f063883272e2` |
| 182 | [wwwroot/assets/images/events/event-040.jpg](../wwwroot/assets/images/events/event-040.jpg) | 152618 | Nhị phân: giải mã ảnh/kích thước | `9896db7a59a72f52e8df16a29159022e0c57d7ccedceb8e5763ddc4c486403e8` |
| 183 | [wwwroot/assets/images/events/event-041.jpg](../wwwroot/assets/images/events/event-041.jpg) | 114225 | Nhị phân: giải mã ảnh/kích thước | `278e03ff07484a2a1b4df6bcbd721799a6c6a8d921d22185dceba5740fdec320` |
| 184 | [wwwroot/assets/images/events/event-042.jpg](../wwwroot/assets/images/events/event-042.jpg) | 114464 | Nhị phân: giải mã ảnh/kích thước | `90a8a5ee11148aa09f6ce74ff73734c6046e4a51c6a63a4c3075967c2136af0a` |
| 185 | [wwwroot/assets/images/events/event-043.jpg](../wwwroot/assets/images/events/event-043.jpg) | 111903 | Nhị phân: giải mã ảnh/kích thước | `2795bdcc9d627c25172055c74749b0687f6b18b095d61765b0cca2f6690dfeab` |
| 186 | [wwwroot/assets/images/events/event-044.jpg](../wwwroot/assets/images/events/event-044.jpg) | 111017 | Nhị phân: giải mã ảnh/kích thước | `6772eebab850f6267ef2c1f11ebbcaaadafb3ba8274192c207ba46cd1e82769d` |
| 187 | [wwwroot/assets/images/events/event-045.jpg](../wwwroot/assets/images/events/event-045.jpg) | 115129 | Nhị phân: giải mã ảnh/kích thước | `4e5dfb60fff2c544934523bf792b18b7017cdcc8d5e0eb7ee004e01b4c8c0890` |
| 188 | [wwwroot/assets/images/events/event-046.jpg](../wwwroot/assets/images/events/event-046.jpg) | 114330 | Nhị phân: giải mã ảnh/kích thước | `80a5a00de26d2e09998bc783cac884f1675488ad84947341970f5ca3f9c3d615` |
| 189 | [wwwroot/assets/images/events/event-047.jpg](../wwwroot/assets/images/events/event-047.jpg) | 115237 | Nhị phân: giải mã ảnh/kích thước | `9b95b57507d38a7f1979f864fedce7b8dcc84e5bf83e0e12355beb4125b17eb4` |
| 190 | [wwwroot/assets/images/events/event-048.jpg](../wwwroot/assets/images/events/event-048.jpg) | 114792 | Nhị phân: giải mã ảnh/kích thước | `105633a5159affcb5ed9049a5ecfb18e321a9826a8250f714d8acecc5609ce7e` |
| 191 | [wwwroot/assets/images/events/event-049.jpg](../wwwroot/assets/images/events/event-049.jpg) | 115705 | Nhị phân: giải mã ảnh/kích thước | `246ad3248a6a1688f0a90f5123115510844f1dc32ccde0ca1d6036b098a8ae37` |
| 192 | [wwwroot/assets/images/events/event-050.jpg](../wwwroot/assets/images/events/event-050.jpg) | 113155 | Nhị phân: giải mã ảnh/kích thước | `226d06abd343326a81d49492c32f78f411c073fb2104a14c7eb75dfa12907442` |
| 193 | [wwwroot/assets/images/events/event-051.jpg](../wwwroot/assets/images/events/event-051.jpg) | 130156 | Nhị phân: giải mã ảnh/kích thước | `4bd1720b78bee55482b12dcbc9841a43b2a744f9b9bb622d3efc33b973eafabb` |
| 194 | [wwwroot/assets/images/events/event-052.jpg](../wwwroot/assets/images/events/event-052.jpg) | 128174 | Nhị phân: giải mã ảnh/kích thước | `54217853e4b45ab81cbfe6e08c833f4296b54c63cb74f6630e145ea0a47026bd` |
| 195 | [wwwroot/assets/images/events/event-053.jpg](../wwwroot/assets/images/events/event-053.jpg) | 129935 | Nhị phân: giải mã ảnh/kích thước | `c9522917ac103ca79e3a588c5ff99ee0b44ae712564ce03037ca9ce9c626298f` |
| 196 | [wwwroot/assets/images/events/event-054.jpg](../wwwroot/assets/images/events/event-054.jpg) | 127656 | Nhị phân: giải mã ảnh/kích thước | `5c964ed378b5be64b9e6e2fdfc31530a0529d04b4eb9bce686200022ddaf10b0` |
| 197 | [wwwroot/assets/images/events/event-055.jpg](../wwwroot/assets/images/events/event-055.jpg) | 126941 | Nhị phân: giải mã ảnh/kích thước | `13df8d664bec408fc4a48e6a8c01073029a4b20e0f621747a59976f90facbe17` |
| 198 | [wwwroot/assets/images/events/event-056.jpg](../wwwroot/assets/images/events/event-056.jpg) | 127355 | Nhị phân: giải mã ảnh/kích thước | `7539637e314f9441a93b6d49df0b85a77e3524550ed991acb3ba69c7c031a4ae` |
| 199 | [wwwroot/assets/images/events/event-057.jpg](../wwwroot/assets/images/events/event-057.jpg) | 128509 | Nhị phân: giải mã ảnh/kích thước | `809ed78512a451a75619f98a46ce70c2eb04ed19676077e8d50081877219a91e` |
| 200 | [wwwroot/assets/images/events/event-058.jpg](../wwwroot/assets/images/events/event-058.jpg) | 127765 | Nhị phân: giải mã ảnh/kích thước | `15e83191dbf240c6de108925ee3eb3cab2c386d32f14938fbdc3240c8df172c7` |
| 201 | [wwwroot/assets/images/events/event-059.jpg](../wwwroot/assets/images/events/event-059.jpg) | 127956 | Nhị phân: giải mã ảnh/kích thước | `8d67630bca6a2bfb8dc46642e957bab9811d22ed8f2a79cfad2a3d4171d88b82` |
| 202 | [wwwroot/assets/images/events/event-060.jpg](../wwwroot/assets/images/events/event-060.jpg) | 128588 | Nhị phân: giải mã ảnh/kích thước | `ba0586b93d3d5965be4428c0c1686a6b6f1e777901e3505df26de6927f9fb556` |
| 203 | [wwwroot/assets/images/events/event-061.jpg](../wwwroot/assets/images/events/event-061.jpg) | 103484 | Nhị phân: giải mã ảnh/kích thước | `91501a89c00df677df58f50eef66eea37ebf1311675b7fdd820af730bfd9d1da` |
| 204 | [wwwroot/assets/images/events/event-062.jpg](../wwwroot/assets/images/events/event-062.jpg) | 105349 | Nhị phân: giải mã ảnh/kích thước | `d04a90e2fb47a44a784c7184afc849e65d8f109a0d7b932eabfcd34df5f3ea42` |
| 205 | [wwwroot/assets/images/events/event-063.jpg](../wwwroot/assets/images/events/event-063.jpg) | 107025 | Nhị phân: giải mã ảnh/kích thước | `607f1f49867173fbab010c4ef7c80eec541cda45c507aeb8f031c784a44dcbea` |
| 206 | [wwwroot/assets/images/events/event-064.jpg](../wwwroot/assets/images/events/event-064.jpg) | 104804 | Nhị phân: giải mã ảnh/kích thước | `9220ed0660c51326f4dea99f8811857a1139f904c68138b8c5a4ad93d39b8177` |
| 207 | [wwwroot/assets/images/events/event-065.jpg](../wwwroot/assets/images/events/event-065.jpg) | 103456 | Nhị phân: giải mã ảnh/kích thước | `f5d7dfb3c82b62a24f0489ff0291eb204f596a59024e594b8f24a705b07bc8e8` |
| 208 | [wwwroot/assets/images/events/event-066.jpg](../wwwroot/assets/images/events/event-066.jpg) | 105811 | Nhị phân: giải mã ảnh/kích thước | `8489e048adcb5474c7fe13a5a2c26350faa824746ba1afe7864e0470754370a3` |
| 209 | [wwwroot/assets/images/events/event-067.jpg](../wwwroot/assets/images/events/event-067.jpg) | 102846 | Nhị phân: giải mã ảnh/kích thước | `4a3297d31a34e795512c57f2f5bea9759d33c6a1995749d679751e73f576fbc6` |
| 210 | [wwwroot/assets/images/events/event-068.jpg](../wwwroot/assets/images/events/event-068.jpg) | 102865 | Nhị phân: giải mã ảnh/kích thước | `663f2bd29190a7a9e16b0c51281b96b1414104655cbd0f841c3d673a0bdcd0be` |
| 211 | [wwwroot/assets/images/events/event-069.jpg](../wwwroot/assets/images/events/event-069.jpg) | 104499 | Nhị phân: giải mã ảnh/kích thước | `6109fe7dc543901fc6a878b43d18232c2441101e318b6ce5997bdfce28f12402` |
| 212 | [wwwroot/assets/images/events/event-070.jpg](../wwwroot/assets/images/events/event-070.jpg) | 102475 | Nhị phân: giải mã ảnh/kích thước | `c9cc4fd5de61f98bd37c9a9e9a40a15c1be8f3896b670f80ee6f218046f7ac69` |
| 213 | [wwwroot/assets/images/events/event-071.jpg](../wwwroot/assets/images/events/event-071.jpg) | 178957 | Nhị phân: giải mã ảnh/kích thước | `292ee2f409385fd2311e7952d22204439e2f3952cf33126dd282cc07cd5e403d` |
| 214 | [wwwroot/assets/images/events/event-072.jpg](../wwwroot/assets/images/events/event-072.jpg) | 178583 | Nhị phân: giải mã ảnh/kích thước | `36596d9fe5cd8b056acb4908bc5e555ea9836e4d8c67fc35f99dacdfff75314a` |
| 215 | [wwwroot/assets/images/events/event-073.jpg](../wwwroot/assets/images/events/event-073.jpg) | 177888 | Nhị phân: giải mã ảnh/kích thước | `56bb9f3e14a1e1755e1c24a28b409fc46d836cfaabebb2ef87ca9226a2d968fe` |
| 216 | [wwwroot/assets/images/events/event-074.jpg](../wwwroot/assets/images/events/event-074.jpg) | 179974 | Nhị phân: giải mã ảnh/kích thước | `5d2acb84273637895226285a873d552331ced31d5f908c964a113ef2c865087b` |
| 217 | [wwwroot/assets/images/events/event-075.jpg](../wwwroot/assets/images/events/event-075.jpg) | 179521 | Nhị phân: giải mã ảnh/kích thước | `f9f9b07f96d0e5d9ed5048b1c24e417f7bdfd07fad04dfb7a8de70d4e9512b6b` |
| 218 | [wwwroot/assets/images/events/event-076.jpg](../wwwroot/assets/images/events/event-076.jpg) | 181237 | Nhị phân: giải mã ảnh/kích thước | `a7b72703133ead8da411fd7c3ae2d44f78345e09d4174cde5d4e91bb558af229` |
| 219 | [wwwroot/assets/images/events/event-077.jpg](../wwwroot/assets/images/events/event-077.jpg) | 180759 | Nhị phân: giải mã ảnh/kích thước | `765b282cd42fd8797510022fe097a87eaf8e5dc452853ca387d7c84e26298380` |
| 220 | [wwwroot/assets/images/events/event-078.jpg](../wwwroot/assets/images/events/event-078.jpg) | 180987 | Nhị phân: giải mã ảnh/kích thước | `26adeea8ff37c7ea202eb224a6d2691cff423fea01480a0bc295b1150143c27d` |
| 221 | [wwwroot/assets/images/events/event-079.jpg](../wwwroot/assets/images/events/event-079.jpg) | 183433 | Nhị phân: giải mã ảnh/kích thước | `90a617ac856c1a0c7e66880c1e75f0848e03276af9668ef7b4a073c36e175b0e` |
| 222 | [wwwroot/assets/images/events/event-080.jpg](../wwwroot/assets/images/events/event-080.jpg) | 181064 | Nhị phân: giải mã ảnh/kích thước | `6b4dd44b8461bc8aad04badf0dcee52979edb51903e3460efc157b708d0e60da` |
| 223 | [wwwroot/assets/images/events/event-081.jpg](../wwwroot/assets/images/events/event-081.jpg) | 151774 | Nhị phân: giải mã ảnh/kích thước | `13961cc59550c60fa99ce91b6c17e862f23a656b3828ef5d0484e46a6b994dc2` |
| 224 | [wwwroot/assets/images/events/event-082.jpg](../wwwroot/assets/images/events/event-082.jpg) | 152466 | Nhị phân: giải mã ảnh/kích thước | `d1f5d0fd795bed52549db345b9748a2bca0c11d3a717067fc25d4477363b002e` |
| 225 | [wwwroot/assets/images/events/event-083.jpg](../wwwroot/assets/images/events/event-083.jpg) | 151263 | Nhị phân: giải mã ảnh/kích thước | `e5d0b9e2b6e0110ab67b73a9e0eefadacf9ffc3d8ba786b0f7f3c9cf6da287be` |
| 226 | [wwwroot/assets/images/events/event-084.jpg](../wwwroot/assets/images/events/event-084.jpg) | 151043 | Nhị phân: giải mã ảnh/kích thước | `3923a67c480379199bd63e235e9fcaaf7ef528597fb725a9d1ec401834bccf41` |
| 227 | [wwwroot/assets/images/events/event-085.jpg](../wwwroot/assets/images/events/event-085.jpg) | 151506 | Nhị phân: giải mã ảnh/kích thước | `c449f466dc0c0ed5533f8385eec08e9a73fb3bf5c0ce07dfab66495231a963f2` |
| 228 | [wwwroot/assets/images/events/event-086.jpg](../wwwroot/assets/images/events/event-086.jpg) | 151879 | Nhị phân: giải mã ảnh/kích thước | `bcca85997eb828139ec191e51aa71419deb57d0e11812d4b7a4bd1144c74761c` |
| 229 | [wwwroot/assets/images/events/event-087.jpg](../wwwroot/assets/images/events/event-087.jpg) | 149679 | Nhị phân: giải mã ảnh/kích thước | `b828ae23c9972383c34d4a249295c7762087555d8f53f5d575f46e8b61799c18` |
| 230 | [wwwroot/assets/images/events/event-088.jpg](../wwwroot/assets/images/events/event-088.jpg) | 149637 | Nhị phân: giải mã ảnh/kích thước | `fd4f86bb3001fdb8877b74548f1b8f73ff9da4742f3397c02c551b65fde2a0f7` |
| 231 | [wwwroot/assets/images/events/event-089.jpg](../wwwroot/assets/images/events/event-089.jpg) | 150161 | Nhị phân: giải mã ảnh/kích thước | `475cc499c40d471d213c9b9ab8d69a29aac310e18d48d4fc80f9328ae8e1d4c6` |
| 232 | [wwwroot/assets/images/events/event-090.jpg](../wwwroot/assets/images/events/event-090.jpg) | 151920 | Nhị phân: giải mã ảnh/kích thước | `7bfc42a388fbcf95cea44a0b3bb9ed93eb5f9425fb959a04e1579dd92954ba13` |
| 233 | [wwwroot/assets/images/events/event-091.jpg](../wwwroot/assets/images/events/event-091.jpg) | 101166 | Nhị phân: giải mã ảnh/kích thước | `8d84592b085944ef26dabe4393ecf6b8fb1f750c1732cca54f8d00afbe33be55` |
| 234 | [wwwroot/assets/images/events/event-092.jpg](../wwwroot/assets/images/events/event-092.jpg) | 100427 | Nhị phân: giải mã ảnh/kích thước | `0b44cd3e888b74c4cad252a600d163ae870e67e8534bac05e8e80e4071f2d0e0` |
| 235 | [wwwroot/assets/images/events/event-093.jpg](../wwwroot/assets/images/events/event-093.jpg) | 100097 | Nhị phân: giải mã ảnh/kích thước | `79a1bf599464ede85341376ab57f89c89dc8f07428385aab9a02af08b2e9ac06` |
| 236 | [wwwroot/assets/images/events/event-094.jpg](../wwwroot/assets/images/events/event-094.jpg) | 100769 | Nhị phân: giải mã ảnh/kích thước | `f1f72f16ecfd8cd75e774556c611113f976abcf285331209141ea620dcc01997` |
| 237 | [wwwroot/assets/images/events/event-095.jpg](../wwwroot/assets/images/events/event-095.jpg) | 97505 | Nhị phân: giải mã ảnh/kích thước | `d33e2f42922c80cb414181040772c99aee40967e624abe3c1c66f01c5d5c4d47` |
| 238 | [wwwroot/assets/images/events/event-096.jpg](../wwwroot/assets/images/events/event-096.jpg) | 98525 | Nhị phân: giải mã ảnh/kích thước | `109b010428c668b8305d71904db6f2f8f650e98de8217984fa12fd5cf6772b99` |
| 239 | [wwwroot/assets/images/events/event-097.jpg](../wwwroot/assets/images/events/event-097.jpg) | 101443 | Nhị phân: giải mã ảnh/kích thước | `adfd401af58e699b3a0dc1b98bef76a41553600bcb02e8cd2c709fd574059ddb` |
| 240 | [wwwroot/assets/images/events/event-098.jpg](../wwwroot/assets/images/events/event-098.jpg) | 96875 | Nhị phân: giải mã ảnh/kích thước | `af9d138257580770e27f4973ee3f412db15f3092a33627ae58914d0657102e85` |
| 241 | [wwwroot/assets/images/events/event-099.jpg](../wwwroot/assets/images/events/event-099.jpg) | 97960 | Nhị phân: giải mã ảnh/kích thước | `3c8d80ae7c4ca49af23a5f766351f02e94d59b54fe5f6ca1d1d5fb4071bac9ee` |
| 242 | [wwwroot/assets/images/events/event-100.jpg](../wwwroot/assets/images/events/event-100.jpg) | 98958 | Nhị phân: giải mã ảnh/kích thước | `8ce1d3424cc680ded56d40ef7c0df6650b0f0b0b723f1337f317c288cd15a26f` |
| 243 | [wwwroot/assets/images/events/sources/art.jpg](../wwwroot/assets/images/events/sources/art.jpg) | 676463 | Nhị phân: giải mã ảnh/kích thước | `efe90067d3ee182507165b64a89b7a32454873ed2c74f1b89a1a249d1a061a1c` |
| 244 | [wwwroot/assets/images/events/sources/business.jpg](../wwwroot/assets/images/events/sources/business.jpg) | 268703 | Nhị phân: giải mã ảnh/kích thước | `72932ff78a7f25f7061070db835878407e7f06faa8297aaa62505186ceb7572d` |
| 245 | [wwwroot/assets/images/events/sources/education.jpg](../wwwroot/assets/images/events/sources/education.jpg) | 214083 | Nhị phân: giải mã ảnh/kích thước | `a0d1de7fda3bb3fd772e88e2d546cddd590b8a3432b206dd9fea0f6efaa05373` |
| 246 | [wwwroot/assets/images/events/sources/family.jpg](../wwwroot/assets/images/events/sources/family.jpg) | 337281 | Nhị phân: giải mã ảnh/kích thước | `a97d27a2eda6503e54d735efbebc089af5183a83ebe58bcde085e335d49bb2ca` |
| 247 | [wwwroot/assets/images/events/sources/food.jpg](../wwwroot/assets/images/events/sources/food.jpg) | 240306 | Nhị phân: giải mã ảnh/kích thước | `096e3f01dba9999c4b29814b8870ee2b8a0433d7ea34eea06a0549f8e93b8574` |
| 248 | [wwwroot/assets/images/events/sources/music.jpg](../wwwroot/assets/images/events/sources/music.jpg) | 368651 | Nhị phân: giải mã ảnh/kích thước | `48598b54a475d3dba75f3799adb41f968d3dd8f4080f6c8ca12337c4dc068ce7` |
| 249 | [wwwroot/assets/images/events/sources/nature.jpg](../wwwroot/assets/images/events/sources/nature.jpg) | 447565 | Nhị phân: giải mã ảnh/kích thước | `f92caff251aa6cbadbb57055fd72e7aa793262802893d55ad2f970dddcb3dd3d` |
| 250 | [wwwroot/assets/images/events/sources/sports.jpg](../wwwroot/assets/images/events/sources/sports.jpg) | 288420 | Nhị phân: giải mã ảnh/kích thước | `2fcb1bc210e641c5fe963a78f9b62f8ae0a9abbb693a8a071bffdd138a1b0aa4` |
| 251 | [wwwroot/assets/images/events/sources/technology.jpg](../wwwroot/assets/images/events/sources/technology.jpg) | 270815 | Nhị phân: giải mã ảnh/kích thước | `892a83a46506a18eb2a9491b2028759783e7c136da47507a6492639bef0d2813` |
| 252 | [wwwroot/assets/images/events/sources/travel.jpg](../wwwroot/assets/images/events/sources/travel.jpg) | 448348 | Nhị phân: giải mã ảnh/kích thước | `5768e080148f013abe3cebc8d5c0c647d0c72c7619941e74b2ba63c89aee086d` |
| 253 | [wwwroot/assets/images/events/SOURCES.md](../wwwroot/assets/images/events/SOURCES.md) | 2042 | Đọc nội dung nguồn/cấu hình/tài liệu | `f988bc31800f6e428e5f4ca2d573cd7ee07c11972c24695e083622634d6dbde0` |
| 254 | [wwwroot/assets/images/placeholder-avatar.svg](../wwwroot/assets/images/placeholder-avatar.svg) | 235 | Đọc XML/SVG | `37bbfdb68883033d9e91e2596235cfd6cf6c9144e18a1e7d30319f0890bc36c7` |
| 255 | [wwwroot/assets/images/placeholder-event.svg](../wwwroot/assets/images/placeholder-event.svg) | 684 | Đọc XML/SVG | `36a873a2797c1f55173e2e56c341e24c917c6957cda0e4765dcc512131776b82` |
| 256 | [wwwroot/assets/images/placeholder-venue.svg](../wwwroot/assets/images/placeholder-venue.svg) | 582 | Đọc XML/SVG | `88a009f8e16a7fb9b1e4bc21005fa838f07265b583d57c4b8604ae3b9e5d7536` |
| 257 | [wwwroot/assets/images/venues/SOURCES.md](../wwwroot/assets/images/venues/SOURCES.md) | 2472 | Đọc nội dung nguồn/cấu hình/tài liệu | `673aa0ecfb9c2fef1b834ca79e7d120f102cc431ff972ef78baffd9c38cacae2` |
| 258 | [wwwroot/assets/images/venues/venue-001.jpg](../wwwroot/assets/images/venues/venue-001.jpg) | 138832 | Nhị phân: giải mã ảnh/kích thước | `8a3a00f46846f04fcba3d5072a7c52a224bad8497b18b92d0841ce6b33a14b39` |
| 259 | [wwwroot/assets/images/venues/venue-002.jpg](../wwwroot/assets/images/venues/venue-002.jpg) | 86547 | Nhị phân: giải mã ảnh/kích thước | `6ecbcb39bcf18abc6ba52f3170487db08024988ddaefe36aff6e757c7ac1ddd1` |
| 260 | [wwwroot/assets/images/venues/venue-003.jpg](../wwwroot/assets/images/venues/venue-003.jpg) | 86049 | Nhị phân: giải mã ảnh/kích thước | `814e6b673274f547ad6b8fcce4fd6826c4681030c6879e0bd03c14bfdfd83450` |
| 261 | [wwwroot/assets/images/venues/venue-004.jpg](../wwwroot/assets/images/venues/venue-004.jpg) | 93656 | Nhị phân: giải mã ảnh/kích thước | `09d7081f2e7761eed2892a4de308124f31c847c94951dc48d5981e3a32b45efe` |
| 262 | [wwwroot/assets/images/venues/venue-005.jpg](../wwwroot/assets/images/venues/venue-005.jpg) | 124239 | Nhị phân: giải mã ảnh/kích thước | `3bed41ab02f0efb7381fcd0c2960240a8f972cccb8f06f87efc45f97eb1b28c5` |
| 263 | [wwwroot/assets/images/venues/venue-006.jpg](../wwwroot/assets/images/venues/venue-006.jpg) | 105473 | Nhị phân: giải mã ảnh/kích thước | `886214bb9345d08ed62293fce52d3eea894c445b5160f885edb269bd13bd119d` |
| 264 | [wwwroot/assets/images/venues/venue-007.jpg](../wwwroot/assets/images/venues/venue-007.jpg) | 76741 | Nhị phân: giải mã ảnh/kích thước | `ca1191792bf9b6c7c07658f3104019c5d42a1c161ba55663ccdff7751332e663` |
| 265 | [wwwroot/assets/images/venues/venue-008.jpg](../wwwroot/assets/images/venues/venue-008.jpg) | 91817 | Nhị phân: giải mã ảnh/kích thước | `7685ae167a9722a9c94b281fdeb17fe9621103104ce37eb19d456ad012cc341f` |
| 266 | [wwwroot/assets/images/venues/venue-009.jpg](../wwwroot/assets/images/venues/venue-009.jpg) | 111227 | Nhị phân: giải mã ảnh/kích thước | `eb09da0f7eca0a2d0ce880d92e5b77b7669d4f04a36921b56dbfb72415935564` |
| 267 | [wwwroot/assets/images/venues/venue-010.jpg](../wwwroot/assets/images/venues/venue-010.jpg) | 98808 | Nhị phân: giải mã ảnh/kích thước | `d1da478e8783ab0d86aaeb1ea8535667d3c4e248a51067c1598d130097d6d4b1` |
| 268 | [wwwroot/config.js](../wwwroot/config.js) | 109 | Đọc nội dung nguồn/cấu hình/tài liệu | `bdd0090a47675dca48e19507b10fdc8624d57c258b44ca05f4c0d549d2ad6756` |
| 269 | [wwwroot/css/app.css](../wwwroot/css/app.css) | 15651 | Đọc nội dung nguồn/cấu hình/tài liệu | `c3a2fcbf8a0fce7150976834bf8929f149b24fb9ab5dc58660038e6395eb2a8b` |
| 270 | [wwwroot/css/base.css](../wwwroot/css/base.css) | 1275 | Đọc nội dung nguồn/cấu hình/tài liệu | `3d373e85d69089e151370eeb16e748a281c3581e0d01e39513dde1767d48ac5b` |
| 271 | [wwwroot/css/content.css](../wwwroot/css/content.css) | 5548 | Đọc nội dung nguồn/cấu hình/tài liệu | `e1ea1c5a1cc7cabf86e1cb0f7fb348ecd60495bfbc697d109448391e558be3b4` |
| 272 | [wwwroot/css/responsive.css](../wwwroot/css/responsive.css) | 15794 | Đọc nội dung nguồn/cấu hình/tài liệu | `8dba7aa7f5fd40b6025082533998a97ebcd525b2695c5e3d6b274af3b3068d50` |
| 273 | [wwwroot/index.html](../wwwroot/index.html) | 7310 | Đọc nội dung nguồn/cấu hình/tài liệu | `965ad872bc1136e167e9d5030ef96bd9b2dd7e21cc254bf4e695ddb8c3a9719c` |
| 274 | [wwwroot/js/admin-catalog.js](../wwwroot/js/admin-catalog.js) | 7172 | Đọc nội dung nguồn/cấu hình/tài liệu | `a2a465e976d3f0775a622f12f030ad8f361ef2c21d5fb217f66f5955cb7a41de` |
| 275 | [wwwroot/js/api.js](../wwwroot/js/api.js) | 11838 | Đọc nội dung nguồn/cấu hình/tài liệu | `c1d7619e625815112827e7d63f4357b1164fba374532ac225c5beaae8a18d3b3` |
| 276 | [wwwroot/js/auth.js](../wwwroot/js/auth.js) | 6683 | Đọc nội dung nguồn/cấu hình/tài liệu | `a245dba785091e196079015da683784292fac54aad5a34fc088504ff933992c6` |
| 277 | [wwwroot/js/checkin.js](../wwwroot/js/checkin.js) | 5256 | Đọc nội dung nguồn/cấu hình/tài liệu | `d4df51f88685c0d255f78091701dfb5f2a22d4670a9cb83d977b98192704c1dd` |
| 278 | [wwwroot/js/compat-multipage.js](../wwwroot/js/compat-multipage.js) | 2494 | Đọc nội dung nguồn/cấu hình/tài liệu | `fe985c02be3ba0cdc24e9012483ed6e6cddf6e7264604d440307a1d80411cfc8` |
| 279 | [wwwroot/js/diadiem.js](../wwwroot/js/diadiem.js) | 10508 | Đọc nội dung nguồn/cấu hình/tài liệu | `dff7d228d71c12f2653ffa5feab807d257cea6f7e61c511ad021b13dc704b2b6` |
| 280 | [wwwroot/js/donve.js](../wwwroot/js/donve.js) | 20473 | Đọc nội dung nguồn/cấu hình/tài liệu | `10d73840725c9d30c94597485a72b8ae4f36d9dc41c25b1bd3b6492f3d338867` |
| 281 | [wwwroot/js/main.js](../wwwroot/js/main.js) | 8551 | Đọc nội dung nguồn/cấu hình/tài liệu | `c4cd0eb83e64c90884a902741224aa0155366e57304929f7e37e0c43e1299a75` |
| 282 | [wwwroot/js/quantri.js](../wwwroot/js/quantri.js) | 2966 | Đọc nội dung nguồn/cấu hình/tài liệu | `cf50cdcabcf5fce38e397cd77630f96e03dddcab7aff2dd2b5794acaeda15cff` |
| 283 | [wwwroot/js/sukien.js](../wwwroot/js/sukien.js) | 15357 | Đọc nội dung nguồn/cấu hình/tài liệu | `918a332a6a099591ce60c634c0c1738086aac08d0cb06b0e743ac024906e9c8d` |
| 284 | [wwwroot/js/tochuc.js](../wwwroot/js/tochuc.js) | 4905 | Đọc nội dung nguồn/cấu hình/tài liệu | `2b6d0dce738288d9ca14b0cfe890b340274b2186eca557ef9e330986e6572646` |
| 285 | [wwwroot/pages/auth/dang-ky.html](../wwwroot/pages/auth/dang-ky.html) | 7276 | Đọc nội dung nguồn/cấu hình/tài liệu | `31a6ce6f2e062aaf76f63be75a3e8197f537f921989eb038855353b748b7d18b` |
| 286 | [wwwroot/pages/auth/dang-nhap.html](../wwwroot/pages/auth/dang-nhap.html) | 9710 | Đọc nội dung nguồn/cấu hình/tài liệu | `82b28af9104f2070e51c52106dbfffe3ce986458051bd11c3d714cf931ec2804` |
| 287 | [wwwroot/pages/ban-to-chuc/su-kien-cua-toi.html](../wwwroot/pages/ban-to-chuc/su-kien-cua-toi.html) | 8631 | Đọc nội dung nguồn/cấu hình/tài liệu | `8fe3b77aa897b8989c8842cae5a5c39efd2504e8673dadb1647b8b69331be195` |
| 288 | [wwwroot/pages/ban-to-chuc/tao-su-kien.html](../wwwroot/pages/ban-to-chuc/tao-su-kien.html) | 9749 | Đọc nội dung nguồn/cấu hình/tài liệu | `a5c39480a022ded43e4e7d0138d1adf950bc65e23f8a4e8fdd284884d27150a8` |
| 289 | [wwwroot/pages/dia-diem/danh-sach.html](../wwwroot/pages/dia-diem/danh-sach.html) | 7012 | Đọc nội dung nguồn/cấu hình/tài liệu | `34ef9d2e2bfabf190c48c51e7175f6d385865bd3ed08a56d048df9f5f2041c93` |
| 290 | [wwwroot/pages/dia-diem/dat-dia-diem.html](../wwwroot/pages/dia-diem/dat-dia-diem.html) | 9473 | Đọc nội dung nguồn/cấu hình/tài liệu | `058ff03238cc76523a7d47c93c78c5def8b7a7e85692e53782dbf7c5b94348df` |
| 291 | [wwwroot/pages/dia-diem/lich-dat.html](../wwwroot/pages/dia-diem/lich-dat.html) | 8569 | Đọc nội dung nguồn/cấu hình/tài liệu | `81b6256354bc518ed293d4bd1c5deb0482f3b43eb7bb46219d0c09f0fd001dd9` |
| 292 | [wwwroot/pages/don-ve/chi-tiet-don.html](../wwwroot/pages/don-ve/chi-tiet-don.html) | 7896 | Đọc nội dung nguồn/cấu hình/tài liệu | `097dc9b9761ba4a6e97b890c85994fb68c851a8e4def2ce0764977374e20d0b5` |
| 293 | [wwwroot/pages/don-ve/don-cua-toi.html](../wwwroot/pages/don-ve/don-cua-toi.html) | 9671 | Đọc nội dung nguồn/cấu hình/tài liệu | `a937e333639279712594b066cb310684f5e31fb60ea0f6da76b6b4f164c5b12c` |
| 294 | [wwwroot/pages/don-ve/tao-don.html](../wwwroot/pages/don-ve/tao-don.html) | 9103 | Đọc nội dung nguồn/cấu hình/tài liệu | `15579852347daea876baf3fa37a0c3b975b57bcf52b388271ce766da25924695` |
| 295 | [wwwroot/pages/errors/404.html](../wwwroot/pages/errors/404.html) | 8160 | Đọc nội dung nguồn/cấu hình/tài liệu | `d0073e14419a1a1831597aad18f5a23594f46a3d5b396a38cb11ab160ac9ca56` |
| 296 | [wwwroot/pages/quan-tri/bao-cao.html](../wwwroot/pages/quan-tri/bao-cao.html) | 9973 | Đọc nội dung nguồn/cấu hình/tài liệu | `127ed7e424b9ad1dea5f5205a239a00148f1a3303cf39c1b1d0a2b36fc52068a` |
| 297 | [wwwroot/pages/quan-tri/dia-diem.html](../wwwroot/pages/quan-tri/dia-diem.html) | 9919 | Đọc nội dung nguồn/cấu hình/tài liệu | `358fe9905678bd771bdec7b7b93ee8ac54411f81e867d91537492e72e82835a8` |
| 298 | [wwwroot/pages/quan-tri/du-lieu.html](../wwwroot/pages/quan-tri/du-lieu.html) | 10221 | Đọc nội dung nguồn/cấu hình/tài liệu | `945b6cf83bae489fe640b2e8a22d4f72e9b2619b46859e44656135dfe2d9af36` |
| 299 | [wwwroot/pages/quan-tri/duyet-dat-dia-diem.html](../wwwroot/pages/quan-tri/duyet-dat-dia-diem.html) | 9132 | Đọc nội dung nguồn/cấu hình/tài liệu | `5a4e0f49f78301216691c982ccfa28d63175be05d38de95a04dc947bf18ff5dc` |
| 300 | [wwwroot/pages/quan-tri/duyet-su-kien.html](../wwwroot/pages/quan-tri/duyet-su-kien.html) | 9508 | Đọc nội dung nguồn/cấu hình/tài liệu | `8453f9888b4d4417b484daf76194667b9443cfdc50216e49ffc563e517b2b255` |
| 301 | [wwwroot/pages/quan-tri/duyet-to-chuc.html](../wwwroot/pages/quan-tri/duyet-to-chuc.html) | 9253 | Đọc nội dung nguồn/cấu hình/tài liệu | `e6936dd701b947c797049f66258495b10d6f04b084a7b28ec8439cec3945b095` |
| 302 | [wwwroot/pages/quan-tri/loai-su-kien.html](../wwwroot/pages/quan-tri/loai-su-kien.html) | 8702 | Đọc nội dung nguồn/cấu hình/tài liệu | `943b2131f6e6693cb1db937a489b205573683652a856d7ce39dfd89e911f9696` |
| 303 | [wwwroot/pages/quan-tri/su-kien-don-theo-tai-khoan.html](../wwwroot/pages/quan-tri/su-kien-don-theo-tai-khoan.html) | 9291 | Đọc nội dung nguồn/cấu hình/tài liệu | `1175a802c57a1865b05d425fbc8f5891a040a7b7afc2cbe96314f5dd63c263fe` |
| 304 | [wwwroot/pages/quan-tri/tai-khoan.html](../wwwroot/pages/quan-tri/tai-khoan.html) | 9490 | Đọc nội dung nguồn/cấu hình/tài liệu | `16a1012291d14b48e93d022706df35c6a53765d72394bc0032eba6a428540dbf` |
| 305 | [wwwroot/pages/quan-tri/tong-quan.html](../wwwroot/pages/quan-tri/tong-quan.html) | 9600 | Đọc nội dung nguồn/cấu hình/tài liệu | `48133107b68d2a056047dd925d29e28bd46cbb3f31fcfcda7f2bd570e6be411d` |
| 306 | [wwwroot/pages/su-kien/chi-tiet.html](../wwwroot/pages/su-kien/chi-tiet.html) | 5754 | Đọc nội dung nguồn/cấu hình/tài liệu | `ac36f2bafa37c4452044c261ae4933c0eed4ad46c38a87f363829424f5faa270` |
| 307 | [wwwroot/pages/su-kien/danh-sach.html](../wwwroot/pages/su-kien/danh-sach.html) | 6338 | Đọc nội dung nguồn/cấu hình/tài liệu | `6088ab1720d3525519bceeb7dd29cfb7f9dab9b56ba140bbf0702f26b1607189` |
| 308 | [wwwroot/pages/tai-khoan/doi-mat-khau.html](../wwwroot/pages/tai-khoan/doi-mat-khau.html) | 10110 | Đọc nội dung nguồn/cấu hình/tài liệu | `32e37baa5ef888f2da7904ec545d84258408b4a2bb28d6ab6e9782f3bc07e87f` |
| 309 | [wwwroot/pages/tai-khoan/thong-tin.html](../wwwroot/pages/tai-khoan/thong-tin.html) | 7512 | Đọc nội dung nguồn/cấu hình/tài liệu | `f21197a0091e86d9f01d137b04eadf7ecf07ad7f2ee8f0a6b46fa4733eb28a54` |
| 310 | [wwwroot/pages/thanh-toan/giao-dich.html](../wwwroot/pages/thanh-toan/giao-dich.html) | 8162 | Đọc nội dung nguồn/cấu hình/tài liệu | `b2ffc8c75124fd573f5693d3a02ed7eb218d6518611c0b1d818066064fee56af` |
| 311 | [wwwroot/pages/thanh-toan/hoan-tien.html](../wwwroot/pages/thanh-toan/hoan-tien.html) | 8269 | Đọc nội dung nguồn/cấu hình/tài liệu | `c9e08bf154a8d1ee7dea48a2ab95515070d80da2a2e11b42f9f193319f2113d1` |
| 312 | [wwwroot/pages/thanh-toan/thanh-toan.html](../wwwroot/pages/thanh-toan/thanh-toan.html) | 6399 | Đọc nội dung nguồn/cấu hình/tài liệu | `6b828f9fab25265717e2900aacb062fb25d9398a6f95be3a80b26a996d413128` |
| 313 | [wwwroot/pages/to-chuc/gui-xac-thuc.html](../wwwroot/pages/to-chuc/gui-xac-thuc.html) | 7323 | Đọc nội dung nguồn/cấu hình/tài liệu | `4eb2d2f6fdb01cfdd763c66c00dc2976d86bb9ac82caca590f4cbc90cb74384d` |
| 314 | [wwwroot/pages/to-chuc/yeu-cau-cua-toi.html](../wwwroot/pages/to-chuc/yeu-cau-cua-toi.html) | 7061 | Đọc nội dung nguồn/cấu hình/tài liệu | `f0a3da2609a4a241de9d8203b7ab0a7de564d8fa6bdf0cefe44f803eb9c9fb19` |
| 315 | [wwwroot/pages/ve/loai-ve.html](../wwwroot/pages/ve/loai-ve.html) | 9694 | Đọc nội dung nguồn/cấu hình/tài liệu | `914898ffbcf9e25e6a11ce73a9c770296f1127baf240897b2975b2994a97edba` |
| 316 | [wwwroot/pages/ve/tra-cuu-check-in.html](../wwwroot/pages/ve/tra-cuu-check-in.html) | 8391 | Đọc nội dung nguồn/cấu hình/tài liệu | `97dd5d2f0bbdf2b9d22b4df68e744647d239392f82a411875794ab521c69b236` |
| 317 | [wwwroot/pages/ve/ve-theo-don.html](../wwwroot/pages/ve/ve-theo-don.html) | 7665 | Đọc nội dung nguồn/cấu hình/tài liệu | `0068b358d82672bc436a46cff5ffe8542df64017c3fc9cce1cf205027bc635db` |
| 318 | [wwwroot/pages/ve/xuat-ve.html](../wwwroot/pages/ve/xuat-ve.html) | 8586 | Đọc nội dung nguồn/cấu hình/tài liệu | `9de5922e98e15d7edb730b5a357840097ddb71aa2a8f7d634b34de8663f61c54` |

## Xác nhận bảo toàn nguồn

So sánh lại SHA-256 của 318 tệp: không có tệp nguồn ban đầu nào bị thay đổi.

Hai tài liệu mới được tạo trong docs: BAO_CAO_PHAN_TICH_HE_THONG.md và PHU_LUC_KIEM_KE_PROJECT.md. Build/test có thể cập nhật bin/obj; không chạy migrate/seed lên database nghiệp vụ và không sửa các thay đổi có sẵn của người dùng.
