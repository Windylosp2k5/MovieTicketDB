using MovieTicketDB.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace MovieTicketDB.Services
{
    public class EmailDeliveryResult
    {
        public bool Sent { get; set; }
        public string Message { get; set; }
    }

    public static class TicketEmailService
    {
        public static EmailDeliveryResult Send(User user, Booking booking, ShowTime showTime)
        {
            var subject = "Vé xem phim " + showTime.Movy.Title + " - " + booking.Code;
            var body = BuildBody(user, booking, showTime);

            if (!IsEnabled())
                return SavePreview(user.Email, subject, body);

            try
            {
                var host = ConfigurationManager.AppSettings["SmtpHost"];
                var port = ParseInt(ConfigurationManager.AppSettings["SmtpPort"], 587);
                var username = ConfigurationManager.AppSettings["SmtpUsername"];
                var password = ConfigurationManager.AppSettings["SmtpPassword"];
                var from = ConfigurationManager.AppSettings["SmtpFrom"];
                var displayName = ConfigurationManager.AppSettings["SmtpDisplayName"] ?? "PHP Cinema";
                var ssl = ParseBool(ConfigurationManager.AppSettings["SmtpEnableSsl"], true);

                using (var message = new MailMessage())
                using (var client = new SmtpClient(host, port))
                {
                    message.From = new MailAddress(from, displayName, Encoding.UTF8);
                    message.To.Add(user.Email);
                    message.Subject = subject;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Body = body;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;

                    client.EnableSsl = ssl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.Send(message);
                }

                return new EmailDeliveryResult { Sent = true, Message = "Vé đã được gửi đến " + user.Email + "." };
            }
            catch (Exception ex)
            {
                var preview = SavePreview(user.Email, subject, body);
                preview.Message = "Không thể gửi email qua SMTP: " + ex.Message + " Bản xem trước đã được lưu trong App_Data/MailDrop.";
                return preview;
            }
        }

        public static EmailDeliveryResult SendStoreOrder(User user, StoreOrder order)
        {
            var subject = "Đơn bắp nước PHP Cinema - " + order.Code;
            var itemRows = new StringBuilder();
            foreach (var item in order.Items)
                itemRows.Append("<tr><td style=\"padding:8px;color:#555\">").Append(E(item.ProductName))
                    .Append(" × ").Append(item.Quantity)
                    .Append("</td><td style=\"padding:8px;text-align:right;font-weight:bold\">")
                    .Append(item.TotalPrice.ToString("N0")).Append("đ</td></tr>");

            var body = @"<!doctype html><html><body style=""margin:0;background:#f4efe7;font-family:Arial,sans-serif;color:#171719"">
<div style=""max-width:680px;margin:30px auto;background:#fff;border-radius:20px;overflow:hidden;box-shadow:0 12px 35px rgba(0,0,0,.12)"">
<div style=""padding:28px 34px;background:#111114;color:#fff""><div style=""font-size:13px;color:#ff7358;font-weight:bold;letter-spacing:2px"">PHP CINEMA STORE</div><h1 style=""margin:8px 0 0"">Đơn hàng đã thanh toán</h1></div>
<div style=""padding:32px 34px""><p>Xin chào <strong>" + E(user.FullName) + @"</strong>,</p>
<p>Hãy đưa mã dưới đây tại quầy PHP Cinema để nhận bắp và nước. Đơn này không yêu cầu vé xem phim.</p>
<div style=""padding:22px;border:2px dashed #f05a3c;border-radius:16px;background:#fff8f4"">
<table style=""width:100%;border-collapse:collapse"">" + itemRows + @"
<tr><td style=""padding:10px 8px;border-top:1px solid #ddd;font-weight:bold"">Tổng tiền</td><td style=""padding:10px 8px;border-top:1px solid #ddd;text-align:right;font-weight:bold;color:#f05a3c;font-size:20px"">" + order.TotalMoney.ToString("N0") + @"đ</td></tr>
</table></div>
<div style=""margin:24px auto 8px;text-align:center;font-size:28px;font-weight:900;letter-spacing:4px"">" + E(order.Code) + @"</div>
<p style=""text-align:center;color:#777;font-size:13px"">Xuất trình mã này để nhận món tại quầy.</p></div></div></body></html>";

            if (!IsEnabled()) return SavePreview(user.Email, subject, body);
            try
            {
                SendMail(user.Email, subject, body);
                return new EmailDeliveryResult { Sent = true, Message = "Xác nhận đơn hàng đã được gửi đến " + user.Email + "." };
            }
            catch (Exception ex)
            {
                var preview = SavePreview(user.Email, subject, body);
                preview.Message = "Không thể gửi email qua SMTP: " + ex.Message + " Bản xem trước đã được lưu trong App_Data/MailDrop.";
                return preview;
            }
        }

        private static string BuildBody(User user, Booking booking, ShowTime show)
        {
            var concessionRows = new StringBuilder();
            if (booking.Concessions != null)
            {
                foreach (var item in booking.Concessions)
                    concessionRows.Append("<tr><td style=\"padding:7px;color:#777\">Bắp &amp; nước</td><td style=\"padding:7px;text-align:right;font-weight:bold\">")
                        .Append(E(item.ProductName)).Append(" × ").Append(item.Quantity)
                        .Append(" · ").Append(item.TotalPrice.ToString("N0")).Append("đ</td></tr>");
            }
            return @"<!doctype html><html><body style=""margin:0;background:#f4efe7;font-family:Arial,sans-serif;color:#171719"">
<div style=""max-width:680px;margin:30px auto;background:#fff;border-radius:20px;overflow:hidden;box-shadow:0 12px 35px rgba(0,0,0,.12)"">
<div style=""padding:28px 34px;background:#111114;color:#fff""><div style=""font-size:13px;color:#ff7358;font-weight:bold;letter-spacing:2px"">PHP CINEMA</div><h1 style=""margin:8px 0 0"">Vé điện tử của bạn</h1></div>
<div style=""padding:32px 34px""><p>Xin chào <strong>" + E(user.FullName) + @"</strong>,</p><p>Thanh toán đã hoàn tất. Hãy xuất trình mã vé dưới đây tại quầy soát vé.</p>
<div style=""padding:22px;border:2px dashed #f05a3c;border-radius:16px;background:#fff8f4"">
<h2 style=""margin:0 0 18px"">" + E(show.Movy.Title) + @"</h2>
<table style=""width:100%;border-collapse:collapse"">
<tr><td style=""padding:7px;color:#777"">Mã vé</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + E(booking.Code) + @"</td></tr>
<tr><td style=""padding:7px;color:#777"">Rạp</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + E(show.Room.Cinema.CinemaName) + @"</td></tr>
<tr><td style=""padding:7px;color:#777"">Phòng / Ghế</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + E(show.Room.RoomName) + " / " + E(string.Join(", ", booking.SeatNames)) + @"</td></tr>
<tr><td style=""padding:7px;color:#777"">Suất chiếu</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + show.StartTime.ToString(@"hh\:mm") + " - " + show.ShowDate.ToString("dd/MM/yyyy") + @"</td></tr>
<tr><td style=""padding:7px;color:#777"">Tiền vé</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + booking.TicketMoney.ToString("N0") + @"đ</td></tr>
" + concessionRows + @"
<tr><td style=""padding:7px;color:#777"">Thanh toán</td><td style=""padding:7px;text-align:right;font-weight:bold"">" + E(booking.PaymentMethod) + @"</td></tr>
<tr><td style=""padding:7px;color:#777"">Tổng tiền</td><td style=""padding:7px;text-align:right;font-weight:bold;color:#f05a3c;font-size:20px"">" + booking.TotalMoney.ToString("N0") + @"đ</td></tr>
</table></div><div style=""margin:24px auto 8px;text-align:center;font-size:28px;font-weight:900;letter-spacing:4px"">" + E(booking.Code) + @"</div>
<p style=""text-align:center;color:#777;font-size:13px"">Email tự động từ PHP Cinema. Vui lòng không trả lời email này.</p></div></div></body></html>";
        }

        private static EmailDeliveryResult SavePreview(string email, string subject, string body)
        {
            var root = HttpContext.Current.Server.MapPath("~/App_Data/MailDrop");
            Directory.CreateDirectory(root);
            var safeName = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Sanitize(email) + ".html";
            File.WriteAllText(Path.Combine(root, safeName), "<!-- " + subject + " -->" + body, Encoding.UTF8);
            return new EmailDeliveryResult { Sent = false, Message = "SMTP chưa được cấu hình. Bản email vé đã lưu tại App_Data/MailDrop/" + safeName + "." };
        }

        private static void SendMail(string email, string subject, string body)
        {
            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var port = ParseInt(ConfigurationManager.AppSettings["SmtpPort"], 587);
            var username = ConfigurationManager.AppSettings["SmtpUsername"];
            var password = ConfigurationManager.AppSettings["SmtpPassword"];
            var from = ConfigurationManager.AppSettings["SmtpFrom"];
            var displayName = ConfigurationManager.AppSettings["SmtpDisplayName"] ?? "PHP Cinema";
            var ssl = ParseBool(ConfigurationManager.AppSettings["SmtpEnableSsl"], true);
            using (var message = new MailMessage())
            using (var client = new SmtpClient(host, port))
            {
                message.From = new MailAddress(from, displayName, Encoding.UTF8);
                message.To.Add(email);
                message.Subject = subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.Body = body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                client.EnableSsl = ssl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);
                client.Send(message);
            }
        }

        private static bool IsEnabled()
        {
            return ParseBool(ConfigurationManager.AppSettings["SmtpEnabled"], false)
                && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpHost"])
                && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpFrom"]);
        }

        private static string E(string value) { return HttpUtility.HtmlEncode(value ?? ""); }
        private static int ParseInt(string value, int fallback) { int result; return int.TryParse(value, out result) ? result : fallback; }
        private static bool ParseBool(string value, bool fallback) { bool result; return bool.TryParse(value, out result) ? result : fallback; }
        private static string Sanitize(string value) { foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_'); return value.Replace("@", "-at-"); }
    }
}
