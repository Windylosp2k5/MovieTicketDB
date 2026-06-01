using MovieTicketDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MovieTicketDB.Controllers
{
    public class MovieController : Controller
    {
        MovieTicketDBEntities db = new MovieTicketDBEntities();

        public ActionResult Index(int? page, int? size)
        {
            int pageSize = size ?? 4;

            int pageNumber = page ?? 1;

            ViewBag.Size = pageSize;

            var movies = db.Movies
                           .OrderBy(m => m.MovieID)
                           .ToPagedList(pageNumber, pageSize);

            return View(movies);
        }

        public ActionResult Detail(int id)
        {
            var movie = db.Movies.FirstOrDefault(m => m.MovieID == id);

            if (movie == null)
            {
                return HttpNotFound();
            }

            return View(movie);
        }

        public ActionResult DetailTicket()
        {
            var ticket = db.Tickets
                           .OrderByDescending(t => t.TicketID)
                           .FirstOrDefault();

            if (ticket == null)
            {
                return HttpNotFound();
            }

            return View(ticket);
        }
        public ActionResult Event()
        {
            var events = db.Events.ToList();

            return View(events);
        }

        public ActionResult EventDetail(int id)
        {
            var ev = db.Events.Find(id);

            if (ev == null)
            {
                return HttpNotFound();
            }

            return View(ev);
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string UserName, string FullName, string Email, string Phone, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var check = db.Users.FirstOrDefault(u => u.UserName == UserName);

            if (check != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            User user = new User();
            user.UserName = UserName;
            user.FullName = FullName;
            user.Email = Email;
            user.Phone = Phone;
            user.Password = Password;
            user.Role = "Customer";

            db.Users.Add(user);
            db.SaveChanges();

            Session["UserID"] = user.UserID;
            Session["UserName"] = user.UserName;
            Session["FullName"] = user.FullName;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            var user = db.Users.FirstOrDefault(u =>
                u.UserName == UserName &&
                u.Password == Password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            Session["UserID"] = user.UserID;
            Session["UserName"] = user.UserName;
            Session["FullName"] = user.FullName;

            return RedirectToAction("Index", "Movie");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Movie");
        }

        public ActionResult BuyTicket(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction("Detail", new { id = id });
        }

        public ActionResult ShowTime(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            var movie = db.Movies.Find(id);

            ViewBag.Movie = movie;

            var showTimes = db.ShowTimes
                              .Where(s => s.MovieID == id)
                              .ToList();

            return View(showTimes);
        }

        public ActionResult SelectSeat(int id)
        {
            var showTime = db.ShowTimes.Find(id);

            ViewBag.ShowTime = showTime;

            var seats = db.Seats
                          .Where(s => s.RoomID == showTime.RoomID)
                          .ToList();

            return View(seats);
        }
        [HttpPost]
        public ActionResult Payment(int showTimeId, string[] seatNames)
        {
            int totalSeats = 0;
            int totalMoney = 0;

            if (seatNames != null)
            {
                foreach (var seat in seatNames)
                {
                    if (seat.Contains("VIP"))
                    {
                        totalSeats += 2;
                        totalMoney += 100000;
                    }
                    else
                    {
                        totalSeats += 1;
                        totalMoney += 50000;
                    }
                }
            }

            ViewBag.SeatNames = seatNames == null ? "Chưa chọn ghế" : string.Join(", ", seatNames);
            ViewBag.TotalSeats = totalSeats;
            ViewBag.TotalMoney = totalMoney;

            return View();
        }
        public ActionResult Theaters()
        {
            return View();
        }

    }
}