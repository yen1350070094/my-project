# 🧋 Hướng dẫn chạy MilkTeaShop v3

## 🚀 Chạy lần đầu hoặc sau khi cập nhật

```bash
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

## 🔑 Tài khoản mẫu (password: 123456)

| Email | Vai trò |
|---|---|
| admin@milktea.com | Admin |
| manager@milktea.com | Manager |
| nhanvien@milktea.com | Employee |
| *(đăng ký mới / Google)* | Customer |

## 🌐 Cấu hình Google OAuth

1. Vào https://console.cloud.google.com
2. Tạo project → APIs & Services → Credentials
3. Create OAuth 2.0 Client ID → Web application
4. Authorized redirect URIs: `https://localhost:XXXX/signin-google`
5. Copy ClientId và ClientSecret vào appsettings.json:

```json
"Authentication": {
  "Google": {
    "ClientId": "xxxx.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-xxxx"
  }
}
```

> Nếu chưa có Google OAuth, đăng ký bình thường vẫn hoạt động.

## 🆕 Tính năng mới v3

- ✅ Lịch sử đơn hàng (Customer: /Order/MyOrders)
- ✅ Theo dõi đơn hàng theo thời gian thực
- ✅ Hủy đơn khi còn Pending
- ✅ Hoàn tiền Momo khi hủy đơn đã thanh toán
- ✅ Tiền mặt → nhân viên xác nhận, Momo → tự động xác nhận
- ✅ Đăng nhập/đăng ký bằng Google
