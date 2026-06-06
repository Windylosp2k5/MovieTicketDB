# PHP Cinema

## Tài khoản mẫu

- Khách hàng: `demo / 123456`
- Quản trị: `admin / admin123`

## Cấu hình gửi vé qua Gmail

Mở `MovieTicketDB/Web.config`, sau đó cập nhật:

```xml
<add key="SmtpEnabled" value="true" />
<add key="SmtpHost" value="smtp.gmail.com" />
<add key="SmtpPort" value="587" />
<add key="SmtpEnableSsl" value="true" />
<add key="SmtpUsername" value="your-email@gmail.com" />
<add key="SmtpPassword" value="your-16-character-app-password" />
<add key="SmtpFrom" value="your-email@gmail.com" />
<add key="SmtpDisplayName" value="PHP Cinema" />
```

Gmail yêu cầu bật xác minh hai bước và tạo **App Password**. Không dùng mật khẩu Gmail thông thường.

Khi `SmtpEnabled=false` hoặc SMTP lỗi, vé vẫn được tạo và bản email HTML được lưu trong:

```text
MovieTicketDB/App_Data/MailDrop
```

## QR thanh toán

Website tạo QR riêng cho ngân hàng, MoMo, ZaloPay và VNPay. Đây là QR mô phỏng phục vụ đồ án. Thanh toán thực tế cần tài khoản merchant, API tạo giao dịch và webhook xác nhận từ từng nhà cung cấp.
