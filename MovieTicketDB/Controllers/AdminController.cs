using MovieTicketDB.Filters;
using MovieTicketDB.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieTicketDB.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View(new AdminDashboardViewModel
            {
                Movies = CinemaStore.Movies.OrderBy(x => x.MovieID).ToList(),
                ShowTimes = CinemaStore.ShowTimes.OrderBy(x => x.ShowDate).ThenBy(x => x.StartTime).Take(30).ToList(),
                Events = CinemaStore.Events.OrderBy(x => x.EventID).ToList(),
                Users = CinemaStore.GetUsers(),
                Bookings = CinemaStore.GetAllBookings(),
                Cinemas = CinemaStore.Cinemas.OrderBy(x => x.CinemaID).ToList(),
                Revenue = CinemaStore.GetAllBookings().Sum(x => x.TotalMoney),
                TotalShowTimes = CinemaStore.ShowTimes.Count
            });
        }

        public ActionResult MovieEdit(int? id)
        {
            var model = id.HasValue ? CinemaStore.Movies.FirstOrDefault(x => x.MovieID == id.Value) : new Movy
            {
                ReleaseDate = DateTime.Today, Rating = 8, Duration = 120, Status = "Đang chiếu", AgeLimit = "T13",
                Language = "Tiếng Việt / Phụ đề", Country = "Mỹ", Format = "2D", Poster = "film1.jpg"
            };
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MovieEdit(Movy model, HttpPostedFileBase posterFile)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Genre))
            {
                ModelState.AddModelError("", "Tên phim và thể loại là bắt buộc.");
                return View(model);
            }
            if (posterFile != null && posterFile.ContentLength > 0)
            {
                var extension = Path.GetExtension(posterFile.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension) || posterFile.ContentLength > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Poster phải là JPG hoặc PNG và không vượt quá 5 MB.");
                    return View(model);
                }
                try
                {
                    using (var image = Image.FromStream(posterFile.InputStream, true, true))
                    {
                        if (image.Width < 200 || image.Height < 300)
                        {
                            ModelState.AddModelError("", "Poster phải có kích thước tối thiểu 200 x 300 px.");
                            return View(model);
                        }
                    }
                    posterFile.InputStream.Position = 0;
                    var fileName = "movie-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
                    var imageDirectory = Server.MapPath("~/Content/images");
                    Directory.CreateDirectory(imageDirectory);
                    posterFile.SaveAs(Path.Combine(imageDirectory, fileName));
                    model.Poster = fileName;
                }
                catch
                {
                    ModelState.AddModelError("", "File được chọn không phải ảnh hợp lệ.");
                    return View(model);
                }
            }
            if (string.IsNullOrWhiteSpace(model.Poster)) model.Poster = "film1.jpg";
            CinemaStore.SaveMovie(model);
            TempData["AdminSuccess"] = "Đã lưu phim " + model.Title + ".";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MovieDelete(int id)
        {
            var blockReason = CinemaStore.GetMovieDeleteBlockReason(id);
            if (!string.IsNullOrWhiteSpace(blockReason))
                TempData["AdminError"] = blockReason;
            else if (CinemaStore.DeleteMovie(id))
                TempData["AdminSuccess"] = "Đã xóa phim.";
            else
                TempData["AdminError"] = "Không tìm thấy phim cần xóa.";
            return RedirectToAction("Index");
        }

        public ActionResult EventEdit(int? id)
        {
            var model = id.HasValue ? CinemaStore.Events.FirstOrDefault(x => x.EventID == id.Value) : new Event
            {
                EndDate = DateTime.Today.AddDays(30), Image = "event1.jpg", Accent = "#e85d3f"
            };
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EventEdit(Event model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("", "Tên sự kiện là bắt buộc.");
                return View(model);
            }
            CinemaStore.SaveEvent(model);
            TempData["AdminSuccess"] = "Đã lưu sự kiện.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EventDelete(int id)
        {
            CinemaStore.DeleteEvent(id);
            TempData["AdminSuccess"] = "Đã xóa sự kiện.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetRole(int id, string role)
        {
            CinemaStore.SetUserRole(id, role);
            TempData["AdminSuccess"] = "Đã cập nhật quyền người dùng.";
            return RedirectToAction("Index");
        }

        public ActionResult CinemaEdit(int? id)
        {
            var model = id.HasValue ? CinemaStore.Cinemas.FirstOrDefault(x => x.CinemaID == id.Value) : new Cinema { Accent = "#e85d3f" };
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CinemaEdit(Cinema model)
        {
            if (string.IsNullOrWhiteSpace(model.CinemaName) || string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("", "Tên và địa chỉ rạp là bắt buộc.");
                return View(model);
            }
            CinemaStore.SaveCinema(model);
            TempData["AdminSuccess"] = "Đã lưu thông tin rạp.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CinemaDelete(int id)
        {
            TempData["AdminSuccess"] = CinemaStore.DeleteCinema(id) ? "Đã xóa rạp và dữ liệu liên quan." : "Không thể xóa rạp cuối cùng.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RebuildShowTimes()
        {
            if (CinemaStore.RebuildShowTimes())
                TempData["AdminSuccess"] = "Đã tạo lại lịch chiếu cho toàn bộ phim.";
            else
                TempData["AdminError"] = "Không thể tạo lại toàn bộ lịch vì đã có vé được đặt. Hãy quản lý từng suất chiếu để bảo toàn lịch sử vé.";
            return RedirectToAction("Index");
        }

        public ActionResult ShowTimeEdit(int? id)
        {
            ViewBag.Movies = CinemaStore.Movies.OrderBy(x => x.Title).ToList();
            ViewBag.Rooms = CinemaStore.Rooms.OrderBy(x => x.Cinema.CinemaName).ThenBy(x => x.RoomName).ToList();
            var model = id.HasValue ? CinemaStore.ShowTimes.FirstOrDefault(x => x.ShowTimeID == id.Value) : new ShowTime
            {
                ShowDate = DateTime.Today, StartTime = new TimeSpan(18, 0, 0), Price = 95000,
                MovieID = CinemaStore.Movies.First().MovieID, RoomID = CinemaStore.Rooms.First().RoomID
            };
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ShowTimeEdit(ShowTime model)
        {
            if (CinemaStore.SaveShowTime(model) == null)
            {
                ModelState.AddModelError("", "Phim hoặc phòng chiếu không hợp lệ.");
                ViewBag.Movies = CinemaStore.Movies;
                ViewBag.Rooms = CinemaStore.Rooms;
                return View(model);
            }
            TempData["AdminSuccess"] = "Đã lưu suất chiếu.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ShowTimeDelete(int id)
        {
            if (CinemaStore.DeleteShowTime(id))
                TempData["AdminSuccess"] = "Đã xóa suất chiếu.";
            else
                TempData["AdminError"] = "Không thể xóa suất chiếu đã có vé hoặc suất chiếu không tồn tại.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetBookingStatus(int id, string status)
        {
            CinemaStore.SetBookingStatus(id, status);
            TempData["AdminSuccess"] = "Đã cập nhật trạng thái vé.";
            return RedirectToAction("Index");
        }
    }
}
