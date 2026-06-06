# Student Score Management API

Project ASP.NET Core Web API (.NET 9) quan ly sinh vien va diem sinh vien theo kien truc nhieu tang:

- `StudentScoreManagement.Api`: Controllers, Middleware, Program, Swagger/JWT config.
- `StudentScoreManagement.Application`: DTOs, service interfaces, pagination/filtering/sorting, response format.
- `StudentScoreManagement.Domain`: Entities va Enums.
- `StudentScoreManagement.Infrastructure`: EF Core DbContext, repositories, services, migrations, CSV import.

## Kien Thuc The Hien

- EF Core ORM: `DbContext`, `DbSet`, Fluent API relationship.
- SQLite file database mac dinh; co the chuyen sang SQL Server khi can.
- Migration SQL Server ban dau tao bang `Students`, `Subjects`, `Scores`, `AppUsers`.
- CRUD bang LINQ va async/await.
- `AsNoTracking()` cho API doc danh sach/chi tiet.
- Paging, filtering, sorting cho students va scores.
- JWT Authentication va role-based Authorization.
- Logging bang Serilog.
- Middleware xu ly exception global.
- CSV la dau vao import ban dau, database moi la noi luu tru chinh.

## Tai Khoan Mac Dinh

Khi app khoi dong, `DbInitializer` se tu tao SQLite database va seed user admin neu chua co user:

- Username: `admin`
- Password: `Admin@123`
- Role: `Admin`

## Cau Hinh Database

Mac dinh project dung SQLite file, khong can cai SQL Server:

```json
"DatabaseProvider": "Sqlite",
"ConnectionStrings": {
  "DefaultConnection": "Data Source=student-score.db"
}
```

File `student-score.db` duoc tao trong thu muc `StudentScoreManagement.Api` khi app khoi dong.
CSV la dau vao import ban dau; SQLite luu du lieu de cac endpoint login va CRUD hoat dong.

Neu muon dung SQL Server Express:

```json
"DatabaseProvider": "SqlServer",
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentScoreManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
```

Co the override cau hinh bang bien moi truong:

```powershell
$env:DatabaseProvider="SqlServer"
$env:ConnectionStrings__DefaultConnection="Server=.\SQLEXPRESS;Database=StudentScoreManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
```

## 1. Restore Package

Mo PowerShell tai thu muc chua file `StudentScoreManagement.sln`, sau do chay:

```powershell
dotnet restore
```

## 2. Update Database

SQLite tu tao schema khi app khoi dong, khong can chay lenh migration.

Neu chuyen sang SQL Server, project da co migration ban dau trong tang Infrastructure:

```powershell
dotnet ef database update `
  --project .\StudentScoreManagement.Infrastructure `
  --startup-project .\StudentScoreManagement.Api
```

Neu may chua co EF tool:

```powershell
dotnet tool install --global dotnet-ef
```

## 3. Chay Project

```powershell
dotnet run --project .\StudentScoreManagement.Api
```

Mo Swagger theo URL hien trong terminal:

```text
http://localhost:5000/swagger
https://localhost:5001/swagger
```

## 4. Login Lay Token

Endpoint:

```http
POST /api/auth/login
```

Body:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Copy `accessToken`, bam nut `Authorize` tren Swagger, chi nhap token (Swagger tu them tien to `Bearer`):

```text
<accessToken>
```

## 5. Import CSV

Endpoint:

```http
POST /api/import/scores-csv
```

Quyen: `Admin`

Body: `multipart/form-data`

- key: `file`
- value: file CSV tren may cua ban, vi du `C:\Users\Admin\Downloads\demodata\DataDemoDiemSV.csv`

CSV can co header. Code import ho tro nhieu ten cot pho bien:

- Sinh vien: `MaSV`, `StudentCode`, `Ma sinh vien`, `Mã sinh viên`
- Ho ten: `HoTen`, `FullName`, `Họ tên`
- Lop: `Lop`, `ClassName`, `Lớp`
- Mon hoc: `MonHoc`, `SubjectName`, `Môn học`
- Ma mon: `MaMon`, `SubjectCode`, `Mã môn`
- Diem: `Diem`, `Score`, `ScoreValue`, `Điểm`
- Hoc ky: `HocKy`, `Semester`, `Học kỳ`
- Nam hoc: `NamHoc`, `SchoolYear`, `Năm học`

Ket qua tra ve:

```json
{
  "success": true,
  "message": "Import CSV successful",
  "data": {
    "totalRows": 100,
    "successRows": 98,
    "failedRows": 2,
    "errors": []
  },
  "errors": []
}
```

## 6. Test Paging / Filtering / Sorting

Students:

```http
GET /api/students?pageNumber=1&pageSize=10&keyword=Nguyen&className=CTK42&sortBy=fullName&sortOrder=asc
```

Scores:

```http
GET /api/scores?pageNumber=1&pageSize=10&studentCode=SV001&className=CTK42&semester=HK1&schoolYear=2025-2026&minScore=5&maxScore=10&sortBy=scoreValue&sortOrder=desc
```

## Phan Quyen Chinh

- `Admin`: CRUD sinh vien, mon hoc, diem, import CSV, quan ly user.
- `Teacher`: xem sinh vien/diem, them/sua diem, khong xoa sinh vien.
- `Student`: chi xem thong tin ca nhan va diem cua chinh minh.

## Response Format

Moi API tra theo format:

```json
{
  "success": true,
  "message": "string",
  "data": {},
  "errors": []
}
```

Danh sach phan trang:

```json
{
  "success": true,
  "message": "string",
  "data": {
    "items": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  },
  "errors": []
}
```
