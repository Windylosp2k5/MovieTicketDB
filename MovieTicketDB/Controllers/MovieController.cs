using MovieTicketDB.Models;
using MovieTicketDB.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using QRCoder;

namespace MovieTicketDB.Controllers
{
    public class MovieController : Controller
    {
        public ActionResult Index(string search, string genre, int? page)
        {
            const int pageSize = 8;
            var pageNumber = page.GetValueOrDefault(1);
            if (pageNumber < 1) pageNumber = 1;

            var movies = CinemaStore.Movies.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                movies = movies.Where(x => x.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || x.Genre.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(genre))
                movies = movies.Where(x => x.Genre.Split(',').Any(g => g.Trim().Equals(genre.Trim(), StringComparison.OrdinalIgnoreCase)));

            return View(new MovieIndexViewModel
            {
                Movies = movies.ToPagedList(pageNumber, pageSize),
                FeaturedMovies = CinemaStore.Movies.Take(3).ToList(),
                Genres = CinemaStore.Movies.SelectMany(x => x.Genre.Split(',')).Select(x => x.Trim()).Distinct().OrderBy(x => x).ToList(),
                HotMovies = CinemaStore.Movies.Where(x => x.IsHot).OrderByDescending(x => x.Rating).Take(6).ToList(),
                UpcomingMovies = CinemaStore.Movies.Where(x => x.Status == "Sắp chiếu").OrderBy(x => x.ReleaseDate).Take(6).ToList(),
                EarlyMovies = CinemaStore.Movies.Where(x => x.IsEarlyAccess).OrderByDescending(x => x.Rating).Take(6).ToList(),
                Events = CinemaStore.Events.OrderBy(x => x.EndDate).ToList(),
                Search = search,
                Genre = genre
            });
        }

        public ActionResult Detail(int id)
        {
            var movie = CinemaStore.Movies.FirstOrDefault(x => x.MovieID == id);
            return movie == null ? (ActionResult)HttpNotFound() : View(movie);
        }

        public ActionResult BuyTicket(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", new { returnUrl = Url.Action("ShowTime", new { id }) });
            return RedirectToAction("ShowTime", new { id });
        }

        public ActionResult ShowTime(int id)
        {
            var movie = CinemaStore.Movies.FirstOrDefault(x => x.MovieID == id);
            if (movie == null) return HttpNotFound();
            ViewBag.Movie = movie;
            return View(CinemaStore.ShowTimes.Where(x => x.MovieID == id).OrderBy(x => x.ShowDate).ThenBy(x => x.StartTime).ToList());
        }

        public ActionResult SelectSeat(int showTimeId)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", new { returnUrl = Url.Action("SelectSeat", new { showTimeId }) });
            var showTime = CinemaStore.ShowTimes.FirstOrDefault(x => x.ShowTimeID == showTimeId);
            if (showTime == null) return HttpNotFound();
            return View(new SeatSelectionViewModel
            {
                ShowTime = showTime,
                Seats = CinemaStore.Seats.Where(x => x.RoomID == showTime.RoomID).ToList(),
                BookedSeats = CinemaStore.GetBookedSeats(showTimeId)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Store(int showTimeId, string[] seatNames)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            var showTime = CinemaStore.ShowTimes.FirstOrDefault(x => x.ShowTimeID == showTimeId);
            if (showTime == null) return HttpNotFound();

            var validSeats = CinemaStore.Seats.Where(x => x.RoomID == showTime.RoomID).Select(x => x.SeatName).ToList();
            var booked = new HashSet<string>(CinemaStore.GetBookedSeats(showTimeId));
            var selected = (seatNames ?? new string[0]).Distinct().Where(x => validSeats.Contains(x) && !booked.Contains(x)).ToList();
            if (!selected.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ghế còn trống.";
                return RedirectToAction("SelectSeat", new { showTimeId });
            }

            var ticketMoney = selected.Sum(x => x.StartsWith("E") ? 120000m : showTime.Price);
            return View(new StoreViewModel
            {
                ShowTime = showTime,
                SeatNames = selected,
                TicketMoney = ticketMoney,
                Products = CinemaStore.Concessions
            });
        }

        public ActionResult StoreMenu()
        {
            return View(CinemaStore.Concessions);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult StoreCheckout(int[] productIds, string[] sizes, int[] quantities)
        {
            var items = BuildConcessionOrder(productIds, sizes, quantities);
            if (!items.Any())
            {
                TempData["StoreError"] = "Vui lòng chọn ít nhất một sản phẩm.";
                return RedirectToAction("StoreMenu");
            }
            if (Session["UserID"] == null)
            {
                Session["PendingStoreCart"] = items;
                return RedirectToAction("Login", new { returnUrl = Url.Action("ResumeStoreCheckout") });
            }

            return CreateStorePayment(items);
        }

        public ActionResult ResumeStoreCheckout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", new { returnUrl = Url.Action("ResumeStoreCheckout") });
            var items = Session["PendingStoreCart"] as List<ConcessionOrderItem>;
            Session.Remove("PendingStoreCart");
            if (items == null || !items.Any()) return RedirectToAction("StoreMenu");
            return CreateStorePayment(items);
        }

        private ActionResult CreateStorePayment(List<ConcessionOrderItem> items)
        {
            var total = items.Sum(x => x.TotalPrice);
            var paymentCode = "PHPSHOP" + DateTime.Now.ToString("yyMMddHHmmss");
            var qrOptions = CreateStorePaymentQrOptions(paymentCode, total, items);
            var model = new StorePaymentViewModel
            {
                Items = items,
                TotalMoney = total,
                PaymentCode = paymentCode,
                QrOptions = qrOptions,
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };
            Session["PendingStorePayment"] = model;
            return View("StorePayment", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ConfirmStorePayment(string paymentMethod, bool paymentConfirmed = false)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            var payment = Session["PendingStorePayment"] as StorePaymentViewModel;
            if (payment == null) return RedirectToAction("StoreMenu");
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                ModelState.AddModelError("", "Vui lòng chọn phương thức thanh toán.");
                return View("StorePayment", payment);
            }
            if (payment.ExpiresAt < DateTime.Now)
            {
                ModelState.AddModelError("", "Mã thanh toán đã hết hạn. Vui lòng tạo lại đơn hàng.");
                return View("StorePayment", payment);
            }
            if (IsQrMethod(paymentMethod) && !paymentConfirmed)
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn đã hoàn tất thanh toán bằng mã QR.");
                return View("StorePayment", payment);
            }

            var paymentName = GetPaymentName(paymentMethod);
            var order = CinemaStore.AddStoreOrder((int)Session["UserID"], payment.Items, paymentName + " - " + payment.PaymentCode);
            var user = CinemaStore.FindUser((int)Session["UserID"]);
            var emailResult = TicketEmailService.SendStoreOrder(user, order);
            Session.Remove("PendingStorePayment");
            TempData["Success"] = "Đặt món thành công. Mã nhận món của bạn là " + order.Code + ".";
            TempData["EmailStatus"] = emailResult.Message;
            TempData["EmailSent"] = emailResult.Sent;
            return RedirectToAction("StoreOrders");
        }

        public ActionResult StoreOrders()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", new { returnUrl = Url.Action("StoreOrders") });
            var user = CinemaStore.FindUser((int)Session["UserID"]);
            return View(CinemaStore.GetStoreOrders(user.UserID).Select(x => new StoreOrderReceiptViewModel { Order = x, User = user }).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Payment(int showTimeId, string[] seatNames, int[] productIds, string[] sizes, int[] quantities)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            var showTime = CinemaStore.ShowTimes.FirstOrDefault(x => x.ShowTimeID == showTimeId);
            if (showTime == null) return HttpNotFound();

            var validSeats = CinemaStore.Seats.Where(x => x.RoomID == showTime.RoomID).Select(x => x.SeatName).ToList();
            var booked = new HashSet<string>(CinemaStore.GetBookedSeats(showTimeId));
            var selected = (seatNames ?? new string[0]).Distinct().Where(x => validSeats.Contains(x) && !booked.Contains(x)).ToList();
            if (!selected.Any())
            {
                TempData["Error"] = "Ghế đã chọn không còn khả dụng. Vui lòng chọn lại.";
                return RedirectToAction("SelectSeat", new { showTimeId });
            }

            var concessions = BuildConcessionOrder(productIds, sizes, quantities);
            var ticketMoney = selected.Sum(x => x.StartsWith("E") ? 120000m : showTime.Price);
            var concessionMoney = concessions.Sum(x => x.TotalPrice);
            var total = ticketMoney + concessionMoney;
            var paymentCode = "PHPPAY" + DateTime.Now.ToString("yyMMddHHmmss") + showTimeId;
            var qrOptions = CreatePaymentQrOptions(paymentCode, total, showTime, selected);
            var model = new PaymentViewModel
            {
                ShowTime = showTime,
                SeatNames = selected,
                TicketMoney = ticketMoney,
                ConcessionMoney = concessionMoney,
                Concessions = concessions,
                TotalMoney = total,
                PaymentCode = paymentCode,
                QrCodeBase64 = qrOptions.First().QrCodeBase64,
                QrOptions = qrOptions,
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };
            Session["PendingPayment"] = model;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(string paymentMethod, bool paymentConfirmed = false)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            var payment = Session["PendingPayment"] as PaymentViewModel;
            if (payment == null) return RedirectToAction("Index");
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                ModelState.AddModelError("", "Vui lòng chọn phương thức thanh toán.");
                return View("Payment", payment);
            }
            if (payment.ExpiresAt < DateTime.Now)
            {
                ModelState.AddModelError("", "Mã thanh toán đã hết hạn. Vui lòng chọn lại ghế để tạo giao dịch mới.");
                return View("Payment", payment);
            }
            if (IsQrMethod(paymentMethod) && !paymentConfirmed)
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn đã hoàn tất thanh toán bằng mã QR.");
                return View("Payment", payment);
            }

            var paymentName = GetPaymentName(paymentMethod);
            var booking = CinemaStore.AddBooking((int)Session["UserID"], payment.ShowTime.ShowTimeID, payment.SeatNames, payment.TicketMoney, payment.Concessions, paymentName + " - " + payment.PaymentCode);
            var user = CinemaStore.FindUser((int)Session["UserID"]);
            var emailResult = TicketEmailService.Send(user, booking, payment.ShowTime);
            Session.Remove("PendingPayment");
            TempData["Success"] = "Thanh toán thành công. Mã vé của bạn là " + booking.Code + ".";
            TempData["EmailStatus"] = emailResult.Message;
            TempData["EmailSent"] = emailResult.Sent;
            return RedirectToAction("DetailTicket");
        }

        public ActionResult DetailTicket()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", new { returnUrl = Url.Action("DetailTicket") });
            var userId = (int)Session["UserID"];
            var user = CinemaStore.FindUser(userId);
            var tickets = CinemaStore.GetBookings(userId).Select(x => new TicketReceiptViewModel
            {
                Booking = x,
                User = user,
                ShowTime = CinemaStore.ShowTimes.First(s => s.ShowTimeID == x.ShowTimeID)
            }).ToList();
            return View(tickets);
        }

        public ActionResult Theaters()
        {
            return View(CinemaStore.Cinemas);
        }

        public ActionResult TheaterSchedule(int id)
        {
            var cinema = CinemaStore.Cinemas.FirstOrDefault(x => x.CinemaID == id);
            if (cinema == null) return HttpNotFound();
            ViewBag.Cinema = cinema;
            return View(CinemaStore.ShowTimes.Where(x => x.Room.CinemaID == id).OrderBy(x => x.ShowDate).ThenBy(x => x.StartTime).ToList());
        }

        public ActionResult Event()
        {
            return View(CinemaStore.Events);
        }

        public ActionResult EventDetail(int id)
        {
            var item = CinemaStore.Events.FirstOrDefault(x => x.EventID == id);
            return item == null ? (ActionResult)HttpNotFound() : View("EventDteail", item);
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string userName, string password, string returnUrl)
        {
            var user = CinemaStore.FindUser(userName, password);
            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            Session["UserID"] = user.UserID;
            Session["UserName"] = user.UserName;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;
            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(string userName, string fullName, string email, string phone, string password, string confirmPassword)
        {
            if (new[] { userName, fullName, email, phone, password }.Any(string.IsNullOrWhiteSpace))
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            else if (password.Length < 6)
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
            else if (password != confirmPassword)
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            else if (CinemaStore.AddUser(new User { UserName = userName.Trim(), FullName = fullName.Trim(), Email = email.Trim(), Phone = phone.Trim(), Password = password }))
            {
                TempData["Success"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
                return RedirectToAction("Login");
            }
            else ViewBag.Error = "Tên đăng nhập hoặc email đã được sử dụng.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        private static string CreateQrCode(string payload)
        {
            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (var code = new QRCode(data))
            using (var bitmap = code.GetGraphic(8, System.Drawing.Color.FromArgb(12, 12, 14), System.Drawing.Color.White, true))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private static string GetPaymentName(string paymentMethod)
        {
            switch (paymentMethod)
            {
                case "QR": return "QR ngân hàng";
                case "MOMO": return "MoMo";
                case "ZALOPAY": return "ZaloPay";
                case "VNPAY": return "VNPay QR";
                default: return "Thanh toán điện tử";
            }
        }

        private static List<PaymentQrOption> CreatePaymentQrOptions(string paymentCode, decimal total, ShowTime showTime, IEnumerable<string> seats)
        {
            var seatText = string.Join(",", seats);
            return new List<PaymentQrOption>
            {
                QrOption("QR", "QR ngân hàng", "PHP Cinema - Vietcombank", "1900 1234 5678", "#247c56", paymentCode, total, showTime, seatText),
                QrOption("MOMO", "Ví MoMo", "PHP Cinema", "0909 888 999", "#a50064", paymentCode, total, showTime, seatText),
                QrOption("ZALOPAY", "Ví ZaloPay", "PHP Cinema", "0909 888 999", "#1677ff", paymentCode, total, showTime, seatText),
                QrOption("VNPAY", "VNPay QR", "PHP Cinema Merchant", "PHP-CINEMA-001", "#e52335", paymentCode, total, showTime, seatText)
            };
        }

        private static List<PaymentQrOption> CreateStorePaymentQrOptions(string paymentCode, decimal total, IEnumerable<ConcessionOrderItem> items)
        {
            var itemText = string.Join(",", items.Select(x => x.ProductName + "x" + x.Quantity));
            return new List<PaymentQrOption>
            {
                StoreQrOption("QR", "QR ngân hàng", "PHP Cinema - Vietcombank", "1900 1234 5678", "#247c56", paymentCode, total, itemText),
                StoreQrOption("MOMO", "Ví MoMo", "PHP Cinema", "0909 888 999", "#a50064", paymentCode, total, itemText),
                StoreQrOption("ZALOPAY", "Ví ZaloPay", "PHP Cinema", "0909 888 999", "#1677ff", paymentCode, total, itemText),
                StoreQrOption("VNPAY", "VNPay QR", "PHP Cinema Merchant", "PHP-CINEMA-001", "#e52335", paymentCode, total, itemText)
            };
        }

        private static PaymentQrOption QrOption(string code, string name, string receiver, string account, string accent, string paymentCode, decimal total, ShowTime showTime, string seats)
        {
            var payload = string.Join("|", new[]
            {
                "PHP CINEMA " + code, "RECEIVER=" + receiver, "ACCOUNT=" + account, "ORDER=" + paymentCode,
                "AMOUNT=" + total.ToString("0"), "MOVIE=" + showTime.Movy.Title, "SEATS=" + seats
            });
            return new PaymentQrOption { Code = code, Name = name, Receiver = receiver, Account = account, Accent = accent, QrCodeBase64 = CreateQrCode(payload) };
        }

        private static PaymentQrOption StoreQrOption(string code, string name, string receiver, string account, string accent, string paymentCode, decimal total, string items)
        {
            var payload = string.Join("|", new[]
            {
                "PHP CINEMA STORE " + code, "RECEIVER=" + receiver, "ACCOUNT=" + account,
                "ORDER=" + paymentCode, "AMOUNT=" + total.ToString("0"), "ITEMS=" + items
            });
            return new PaymentQrOption { Code = code, Name = name, Receiver = receiver, Account = account, Accent = accent, QrCodeBase64 = CreateQrCode(payload) };
        }

        private static bool IsQrMethod(string paymentMethod)
        {
            return new[] { "QR", "MOMO", "ZALOPAY", "VNPAY" }.Contains(paymentMethod);
        }

        private static List<ConcessionOrderItem> BuildConcessionOrder(int[] productIds, string[] sizes, int[] quantities)
        {
            var result = new List<ConcessionOrderItem>();
            var count = new[] { productIds == null ? 0 : productIds.Length, sizes == null ? 0 : sizes.Length, quantities == null ? 0 : quantities.Length }.Min();
            for (var i = 0; i < count; i++)
            {
                var product = CinemaStore.Concessions.FirstOrDefault(x => x.ProductID == productIds[i]);
                var quantity = Math.Max(0, Math.Min(10, quantities[i]));
                if (product == null || quantity == 0) continue;
                result.Add(new ConcessionOrderItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.Name,
                    Size = "",
                    Quantity = quantity,
                    UnitPrice = product.BasePrice
                });
            }
            return result;
        }
    }
}
