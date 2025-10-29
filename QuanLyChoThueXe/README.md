# 🚗 Hệ Thống Quản Lý Cho Thuê Xe

## 📋 Mô tả dự án

Hệ thống quản lý cho thuê xe được phát triển bằng ASP.NET Core MVC 8.0, hỗ trợ đầy đủ các chức năng quản lý xe, khách hàng, đặt xe trước, hợp đồng thuê và thông báo tự động.

## ✨ Tính năng chính

### 🚙 Quản lý xe (US-01)
- ✅ Thêm, sửa, xóa, xem chi tiết xe
- ✅ Tra cứu và lọc xe theo trạng thái
- ✅ Quản lý thông tin đầy đủ: tên, biển số, hãng, model, giá thuê
- ✅ Thống kê số lần thuê và doanh thu

### 🔍 Tra cứu tình trạng xe (US-02)
- ✅ Hiển thị trạng thái: Có sẵn, Đang thuê, Đã đặt trước, Bảo trì, Ngừng hoạt động
- ✅ API để lấy thông tin trạng thái xe
- ✅ Dashboard thống kê trạng thái xe

### 👥 Quản lý khách hàng (US-03)
- ✅ Lưu trữ hồ sơ khách hàng đầy đủ
- ✅ Quản lý giấy tờ tùy thân (CCCD/CMND)
- ✅ Lịch sử thuê xe của khách hàng

### 📅 Đặt xe trước (US-04)
- ✅ Ghi nhận yêu cầu đặt xe
- ✅ Hủy đặt nếu không còn nhu cầu
- ✅ Tự động tính giá thuê
- ✅ Cập nhật trạng thái xe khi đặt

### 🔔 Thông báo tự động (US-05)
- ✅ Hệ thống thông báo tự động
- ✅ Gửi thông báo khi xe có sẵn cho khách đã đặt trước
- ✅ Quản lý trạng thái đọc/chưa đọc

### 📄 Hợp đồng thuê xe (US-06)
- ✅ Tạo hợp đồng với số hợp đồng tự động
- ✅ Hỗ trợ nhiều loại xe và số lượng
- ✅ Tính toán giá thuê và phí đặt cọc
- ✅ In hợp đồng và ký số

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server LocalDB với Entity Framework Core
- **Frontend**: Bootstrap 5, Font Awesome, jQuery
- **Architecture**: MVC Pattern với Repository Pattern

## 🚀 Hướng dẫn cài đặt và chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- Visual Studio 2022 hoặc VS Code
- SQL Server LocalDB (tự động cài đặt với Visual Studio)

### Các bước cài đặt

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd QuanLyChoThueXe
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Tạo database**
   ```bash
   dotnet ef database update
   ```

4. **Chạy ứng dụng**
   ```bash
   dotnet run
   ```

5. **Truy cập ứng dụng**
   - Mở trình duyệt và truy cập: `https://localhost:7150` hoặc `http://localhost:5123`

### Chạy trên Visual Studio 2022

1. Mở file `QuanLyChoThueXe.sln` trong Visual Studio 2022
2. Chọn profile "QuanLyChoThueXe" trong dropdown debug
3. Nhấn F5 hoặc Ctrl+F5 để chạy

## 📊 Cấu trúc database

### Bảng Cars
- Id, Name, LicensePlate, Brand, Model, Year
- Type, Seats, FuelType, PricePerDay
- Description, ImageUrl, Status
- CreatedAt, UpdatedAt

### Bảng Customers
- Id, FullName, PhoneNumber, Email
- IdentityNumber, DateOfBirth, Gender
- Address, Occupation, Workplace, Status
- CreatedAt, UpdatedAt

### Bảng Bookings
- Id, CustomerId, CarId
- StartDate, EndDate, RentalDays, TotalPrice
- Notes, Status, CreatedAt, UpdatedAt

### Bảng RentalContracts
- Id, CustomerId, CarId, ContractNumber
- StartDate, EndDate, RentalDays
- PricePerDay, TotalPrice, Deposit
- Terms, Notes, Status
- CreatedAt, UpdatedAt, SignedAt

### Bảng Notifications
- Id, CustomerId, CarId
- Title, Content, Type, Status
- CreatedAt, ReadAt

## 🎨 Giao diện

- **Thiết kế hiện đại** theo phong cách Mioto.vn
- **Dashboard tổng quan** với thống kê trực quan
- **Sidebar navigation** với các chức năng chính
- **Responsive design** tương thích mobile
- **Icons và màu sắc** chuyên nghiệp

## 📱 Dữ liệu mẫu

Hệ thống đã được tạo sẵn dữ liệu mẫu:
- **3 xe**: Toyota Vios, Honda CR-V, Ford Ranger
- **2 khách hàng**: Nguyễn Văn A, Trần Thị B

## 🔧 Troubleshooting

### Lỗi "The system cannot find the file specified"
- Đảm bảo đã remove cấu hình Container debugging trong csproj
- Clean và rebuild project
- Kiểm tra launchSettings.json

### Lỗi database connection
- Đảm bảo SQL Server LocalDB đã được cài đặt
- Chạy `dotnet ef database update` để tạo database
- Kiểm tra connection string trong appsettings.json

### Lỗi build
- Chạy `dotnet clean` và `dotnet build`
- Kiểm tra .NET 8.0 SDK đã được cài đặt
- Restore packages với `dotnet restore`

## 📞 Hỗ trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra log trong console
2. Xem lại cấu hình trong appsettings.json
3. Đảm bảo tất cả dependencies đã được cài đặt

## 📄 License

Dự án được phát triển cho mục đích học tập và nghiên cứu.

---

**🎉 Hệ thống đã hoàn thành đầy đủ theo yêu cầu Sprint Planning!**




