using MovieTicketDB.Filters;
using MovieTicketDB.Models;
using System;
using System.Linq;
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
                Language = "Tiếng Việt / Phụ đề", Country = "Mỹ", Format = "2D", Poster = "movie1.jpg"
            };
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MovieEdit(Movy model)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Genre))
            {
                ModelState.AddModelError("", "Tên phim và thể loại là bắt buộc.");
                return View(model);
            }
            CinemaStore.SaveMovie(model);
            TempData["AdminSuccess"] = "Đã lưu phim " + model.Title + ".";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MovieDelete(int id)
        {
            CinemaStore.DeleteMovie(id);
            TempData["AdminSuccess"] = "Đã xóa phim.";
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
            CinemaStore.RebuildShowTimes();
            TempData["AdminSuccess"] = "Đã tạo lại lịch chiếu cho toàn bộ phim.";
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
            CinemaStore.DeleteShowTime(id);
            TempData["AdminSuccess"] = "Đã xóa suất chiếu.";
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
