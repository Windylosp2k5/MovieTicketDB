using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MovieTicketDB.Models
{
    public static class CinemaStore
    {
        private static readonly object SyncRoot = new object();

        public static readonly List<Movy> Movies = new List<Movy>
        {
            Movie(1, "Dune: Hành Tinh Cát", "Khoa học viễn tưởng, Phiêu lưu", 166, "Denis Villeneuve", "Timothée Chalamet, Zendaya", "Paul Atreides bước vào hành trình định mệnh giữa sa mạc Arrakis, nơi quyền lực, gia đình và tương lai của cả thiên hà va chạm.", 8.8m, "T13", true, false),
            Movie(2, "Mai", "Tình cảm, Tâm lý", 131, "Trấn Thành", "Phương Anh Đào, Tuấn Trần", "Một câu chuyện trưởng thành nhiều cảm xúc về những người đi qua tổn thương và học cách trao cho bản thân cơ hội hạnh phúc.", 8.4m, "T18", true, false),
            Movie(3, "Kung Fu Panda 4", "Hoạt hình, Phiêu lưu", 94, "Mike Mitchell", "Jack Black, Awkwafina", "Po chuẩn bị trở thành thủ lĩnh tinh thần của Thung lũng Bình Yên và đối đầu một kẻ thù có khả năng triệu hồi quá khứ.", 8.1m, "P", true, false),
            Movie(4, "Godzilla x Kong", "Hành động, Giả tưởng", 115, "Adam Wingard", "Rebecca Hall, Brian Tyree Henry", "Hai Titan huyền thoại buộc phải hợp sức trước hiểm họa khổng lồ ẩn sâu trong Trái Đất Rỗng.", 8.0m, "T13", true, true),
            Movie(5, "Lật Mặt 7", "Gia đình, Chính kịch", 138, "Lý Hải", "Thanh Hiền, Trương Minh Cường", "Bức tranh gia đình gần gũi về tình mẫu tử, khoảng cách thế hệ và giá trị của những lần trở về.", 8.6m, "K", true, false),
            Movie(6, "Inside Out 2", "Hoạt hình, Gia đình", 96, "Kelsey Mann", "Amy Poehler, Maya Hawke", "Riley bước vào tuổi thiếu niên và Trung tâm Cảm xúc phải chào đón những cảm xúc hoàn toàn mới.", 8.7m, "P", true, false),
            Movie(7, "Furiosa", "Hành động, Phiêu lưu", 148, "George Miller", "Anya Taylor-Joy, Chris Hemsworth", "Nguồn gốc của chiến binh Furiosa được hé lộ trong một thế giới hậu tận thế khốc liệt và rực lửa.", 8.3m, "T18", true, true),
            Movie(8, "A Quiet Place: Day One", "Kinh dị, Giật gân", 99, "Michael Sarnoski", "Lupita Nyong'o, Joseph Quinn", "Ngày đầu tiên của cuộc xâm lăng buộc những người sống sót phải giữ im lặng tuyệt đối giữa New York hỗn loạn.", 7.9m, "T16", false, false),
            Movie(9, "Deadpool & Wolverine", "Hành động, Hài", 128, "Shawn Levy", "Ryan Reynolds, Hugh Jackman", "Deadpool kéo Wolverine vào một nhiệm vụ hỗn loạn có thể thay đổi dòng thời gian của cả hai.", 8.5m, "T18", true, true),
            Movie(10, "Despicable Me 4", "Hoạt hình, Hài", 95, "Chris Renaud", "Steve Carell, Kristen Wiig", "Gia đình Gru đón thành viên mới và phải đối đầu với một siêu phản diện đầy ám ảnh.", 8.0m, "P", false, false),
            Movie(11, "Twisters", "Hành động, Thảm họa", 122, "Lee Isaac Chung", "Daisy Edgar-Jones, Glen Powell", "Những thợ săn bão thử nghiệm công nghệ mới giữa mùa lốc xoáy dữ dội chưa từng thấy.", 7.8m, "T13", false, true),
            Movie(12, "Alien: Romulus", "Kinh dị, Khoa học viễn tưởng", 119, "Fede Álvarez", "Cailee Spaeny, David Jonsson", "Một nhóm người trẻ đối mặt sinh vật đáng sợ nhất vũ trụ trong trạm không gian bỏ hoang.", 8.2m, "T18", true, false),
            Movie(13, "Joker: Folie à Deux", "Tâm lý, Âm nhạc", 138, "Todd Phillips", "Joaquin Phoenix, Lady Gaga", "Arthur Fleck gặp Harleen Quinzel và bước vào câu chuyện tình yêu méo mó giữa Gotham.", 8.1m, "T18", false, true),
            Movie(14, "Venom: The Last Dance", "Hành động, Khoa học viễn tưởng", 110, "Kelly Marcel", "Tom Hardy, Juno Temple", "Eddie và Venom bị truy đuổi bởi cả hai thế giới trong chương cuối của hành trình cộng sinh.", 7.9m, "T13", false, true),
            Movie(15, "Moana 2", "Hoạt hình, Phiêu lưu", 100, "David Derrick Jr.", "Auli'i Cravalho, Dwayne Johnson", "Moana nhận lời gọi từ tổ tiên và lên đường khám phá vùng biển xa xôi cùng Maui.", 8.3m, "P", false, false),
            Movie(16, "Gladiator II", "Hành động, Chính kịch", 148, "Ridley Scott", "Paul Mescal, Pedro Pascal", "Nhiều năm sau cái chết của Maximus, Lucius bước vào đấu trường để giành lại danh dự.", 8.4m, "T18", true, true),
            Movie(17, "Mufasa: The Lion King", "Gia đình, Phiêu lưu", 118, "Barry Jenkins", "Aaron Pierre, Kelvin Harrison Jr.", "Nguồn gốc của Mufasa được kể lại qua tình bạn, mất mát và hành trình trở thành vua.", 8.2m, "P", false, false),
            Movie(18, "Sonic the Hedgehog 3", "Hành động, Gia đình", 110, "Jeff Fowler", "Ben Schwartz, Jim Carrey", "Sonic, Tails và Knuckles đối đầu Shadow trong cuộc phiêu lưu tốc độ cao.", 8.0m, "P", false, true),
            Movie(19, "The Wild Robot", "Hoạt hình, Gia đình", 102, "Chris Sanders", "Lupita Nyong'o, Pedro Pascal", "Robot Roz lạc trên đảo hoang, học cách thích nghi và xây dựng gia đình với các loài vật.", 8.8m, "P", true, false),
            Movie(20, "Wicked", "Âm nhạc, Giả tưởng", 160, "Jon M. Chu", "Cynthia Erivo, Ariana Grande", "Tình bạn giữa Elphaba và Glinda trước khi họ trở thành hai phù thủy nổi tiếng xứ Oz.", 8.6m, "K", true, true)
        };

        public static readonly List<Cinema> Cinemas = new List<Cinema>
        {
            new Cinema { CinemaID = 1, CinemaName = "PHP Cinema Nguyễn Huệ", Address = "123 Nguyễn Huệ, Quận 1, TP.HCM", Description = "Cụm rạp trung tâm với phòng chiếu Laser và âm thanh Dolby Atmos.", Accent = "#e85d3f" },
            new Cinema { CinemaID = 2, CinemaName = "PHP Cinema Gò Vấp", Address = "456 Quang Trung, Gò Vấp, TP.HCM", Description = "Không gian trẻ trung, khu chờ rộng và phòng chiếu ghế đôi.", Accent = "#7a5ce6" },
            new Cinema { CinemaID = 3, CinemaName = "PHP Cinema Thủ Đức", Address = "789 Võ Văn Ngân, TP. Thủ Đức", Description = "Điểm hẹn điện ảnh hiện đại dành cho sinh viên và gia đình.", Accent = "#209b87" }
        };

        public static readonly List<Room> Rooms = new List<Room>();
        public static readonly List<ShowTime> ShowTimes = new List<ShowTime>();
        public static readonly List<Seat> Seats = new List<Seat>();
        public static readonly List<Event> Events = new List<Event>
        {
            Event(1, "Thứ Hai Đồng Giá 69K", "MỖI THỨ HAI", "Tận hưởng mọi bộ phim 2D với mức giá ưu đãi 69.000đ dành cho thành viên.", "event1.jpg", "#e85d3f", 60),
            Event(2, "Combo Hẹn Hò", "ƯU ĐÃI CẶP ĐÔI", "Hai vé ghế đôi, một bắp lớn và hai nước với giá chỉ từ 299.000đ.", "event2.jpg", "#d14f77", 45),
            Event(3, "Sinh Nhật Rực Rỡ", "QUÀ THÀNH VIÊN", "Tặng một vé 2D trong tháng sinh nhật dành cho thành viên thân thiết.", "event3.jpg", "#7658d8", 90),
            Event(4, "Suất Chiếu Sớm", "ĐẶC QUYỀN PREMIERE", "Xem trước bom tấn được mong chờ cùng quà tặng giới hạn tại PHP Cinema.", "event4.jpg", "#209b87", 30)
        };

        private static readonly List<User> Users = new List<User>
        {
            new User { UserID = 1, UserName = "demo", FullName = "Khách hàng PHP Cinema", Email = "demo@phpcinema.vn", Phone = "0900000000", Password = HashPassword("123456"), Role = "Customer" },
            new User { UserID = 2, UserName = "admin", FullName = "Quản trị viên PHP Cinema", Email = "admin@phpcinema.vn", Phone = "0909999999", Password = HashPassword("admin123"), Role = "Admin" }
        };

        private static readonly List<Booking> Bookings = new List<Booking>();

        static CinemaStore()
        {
            var roomId = 1;
            foreach (var cinema in Cinemas)
                for (var i = 1; i <= 2; i++)
                    Rooms.Add(new Room { RoomID = roomId++, CinemaID = cinema.CinemaID, Cinema = cinema, RoomName = i == 1 ? "Premium 01" : "Standard 02" });

            var seatId = 1;
            foreach (var room in Rooms)
                foreach (var row in new[] { "A", "B", "C", "D", "E" })
                    for (var number = 1; number <= 8; number++)
                        Seats.Add(new Seat { SeatID = seatId++, RoomID = room.RoomID, SeatName = row + number, Type = row == "E" ? "VIP" : "Standard" });

            RebuildShowTimes();
        }

        public static void RebuildShowTimes()
        {
            lock (SyncRoot)
            {
                ShowTimes.Clear();
                var id = 1;
                for (var day = 0; day < 7; day++)
                    foreach (var movie in Movies)
                    {
                        var room = Rooms[(movie.MovieID + day) % Rooms.Count];
                        foreach (var hour in new[] { 10, 14, 18, 21 })
                            ShowTimes.Add(new ShowTime
                            {
                                ShowTimeID = id++, MovieID = movie.MovieID, RoomID = room.RoomID, Movy = movie, Room = room,
                                ShowDate = DateTime.Today.AddDays(day), StartTime = new TimeSpan(hour, movie.MovieID % 2 == 0 ? 30 : 0, 0),
                                Price = hour >= 18 ? 95000 : 75000
                            });
                    }
            }
        }

        public static User FindUser(string userName, string password)
        {
            var hash = HashPassword(password);
            lock (SyncRoot) return Users.FirstOrDefault(x => x.UserName.Equals(userName ?? "", StringComparison.OrdinalIgnoreCase) && x.Password == hash);
        }

        public static User FindUser(int id) { lock (SyncRoot) return Users.FirstOrDefault(x => x.UserID == id); }
        public static List<User> GetUsers() { lock (SyncRoot) return Users.Select(CloneUser).ToList(); }
        public static List<Booking> GetAllBookings() { lock (SyncRoot) return Bookings.OrderByDescending(x => x.BookingDate).ToList(); }

        public static bool AddUser(User user)
        {
            lock (SyncRoot)
            {
                if (Users.Any(x => x.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) || x.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) return false;
                user.UserID = Users.Max(x => x.UserID) + 1; user.Password = HashPassword(user.Password); user.Role = "Customer"; Users.Add(user); return true;
            }
        }

        public static bool SetUserRole(int id, string role)
        {
            lock (SyncRoot)
            {
                var user = Users.FirstOrDefault(x => x.UserID == id);
                if (user == null || user.UserName == "admin") return false;
                user.Role = role == "Admin" ? "Admin" : "Customer";
                return true;
            }
        }

        public static Movy SaveMovie(Movy input)
        {
            lock (SyncRoot)
            {
                var movie = Movies.FirstOrDefault(x => x.MovieID == input.MovieID);
                if (movie == null)
                {
                    input.MovieID = Movies.Any() ? Movies.Max(x => x.MovieID) + 1 : 1;
                    input.Poster = string.IsNullOrWhiteSpace(input.Poster) ? "movie1.jpg" : input.Poster;
                    input.Cast = BuildCast(input.MovieID, input.Actors);
                    Movies.Add(input);
                    RebuildShowTimes();
                    return input;
                }
                movie.Title = input.Title; movie.Genre = input.Genre; movie.Duration = input.Duration; movie.Director = input.Director;
                movie.Actors = input.Actors; movie.Description = input.Description; movie.AgeLimit = input.AgeLimit; movie.Rating = input.Rating;
                movie.Status = input.Status; movie.Poster = input.Poster; movie.ReleaseDate = input.ReleaseDate; movie.Country = input.Country;
                movie.Format = input.Format; movie.IsHot = input.IsHot; movie.IsEarlyAccess = input.IsEarlyAccess; movie.Cast = BuildCast(movie.MovieID, movie.Actors);
                return movie;
            }
        }

        public static bool DeleteMovie(int id)
        {
            lock (SyncRoot)
            {
                var movie = Movies.FirstOrDefault(x => x.MovieID == id);
                if (movie == null) return false;
                Movies.Remove(movie); RebuildShowTimes(); return true;
            }
        }

        public static Event SaveEvent(Event input)
        {
            lock (SyncRoot)
            {
                var item = Events.FirstOrDefault(x => x.EventID == input.EventID);
                if (item == null) { input.EventID = Events.Any() ? Events.Max(x => x.EventID) + 1 : 1; Events.Add(input); return input; }
                item.Title = input.Title; item.Label = input.Label; item.Description = input.Description; item.Image = input.Image;
                item.Accent = input.Accent; item.EndDate = input.EndDate; return item;
            }
        }

        public static bool DeleteEvent(int id) { lock (SyncRoot) { var item = Events.FirstOrDefault(x => x.EventID == id); if (item == null) return false; Events.Remove(item); return true; } }
        public static Cinema SaveCinema(Cinema input)
        {
            lock (SyncRoot)
            {
                var item = Cinemas.FirstOrDefault(x => x.CinemaID == input.CinemaID);
                if (item != null)
                {
                    item.CinemaName = input.CinemaName; item.Address = input.Address; item.Description = input.Description; item.Accent = input.Accent;
                    return item;
                }
                input.CinemaID = Cinemas.Any() ? Cinemas.Max(x => x.CinemaID) + 1 : 1;
                Cinemas.Add(input);
                for (var i = 1; i <= 2; i++)
                {
                    var room = new Room { RoomID = Rooms.Any() ? Rooms.Max(x => x.RoomID) + 1 : 1, CinemaID = input.CinemaID, Cinema = input, RoomName = i == 1 ? "Premium 01" : "Standard 02" };
                    Rooms.Add(room);
                    foreach (var row in new[] { "A", "B", "C", "D", "E" })
                        for (var number = 1; number <= 8; number++)
                            Seats.Add(new Seat { SeatID = Seats.Any() ? Seats.Max(x => x.SeatID) + 1 : 1, RoomID = room.RoomID, SeatName = row + number, Type = row == "E" ? "VIP" : "Standard" });
                }
                return input;
            }
        }
        public static bool DeleteCinema(int id)
        {
            lock (SyncRoot)
            {
                if (Cinemas.Count <= 1) return false;
                var item = Cinemas.FirstOrDefault(x => x.CinemaID == id);
                if (item == null) return false;
                var roomIds = Rooms.Where(x => x.CinemaID == id).Select(x => x.RoomID).ToList();
                ShowTimes.RemoveAll(x => roomIds.Contains(x.RoomID)); Seats.RemoveAll(x => roomIds.Contains(x.RoomID)); Rooms.RemoveAll(x => roomIds.Contains(x.RoomID)); Cinemas.Remove(item);
                return true;
            }
        }
        public static ShowTime SaveShowTime(ShowTime input)
        {
            lock (SyncRoot)
            {
                var movie = Movies.FirstOrDefault(x => x.MovieID == input.MovieID);
                var room = Rooms.FirstOrDefault(x => x.RoomID == input.RoomID);
                if (movie == null || room == null) return null;
                var item = ShowTimes.FirstOrDefault(x => x.ShowTimeID == input.ShowTimeID);
                if (item == null)
                {
                    input.ShowTimeID = ShowTimes.Any() ? ShowTimes.Max(x => x.ShowTimeID) + 1 : 1;
                    input.Movy = movie; input.Room = room; ShowTimes.Add(input); return input;
                }
                item.MovieID = input.MovieID; item.RoomID = input.RoomID; item.Movy = movie; item.Room = room;
                item.ShowDate = input.ShowDate; item.StartTime = input.StartTime; item.Price = input.Price; return item;
            }
        }
        public static bool DeleteShowTime(int id) { lock (SyncRoot) { var item = ShowTimes.FirstOrDefault(x => x.ShowTimeID == id); if (item == null) return false; ShowTimes.Remove(item); return true; } }
        public static bool SetBookingStatus(int id, string status)
        {
            lock (SyncRoot)
            {
                var item = Bookings.FirstOrDefault(x => x.BookingID == id);
                if (item == null) return false;
                item.Status = new[] { "Đã thanh toán", "Đã sử dụng", "Đã hủy" }.Contains(status) ? status : item.Status;
                return true;
            }
        }
        public static IEnumerable<string> GetBookedSeats(int showTimeId) { lock (SyncRoot) return Bookings.Where(x => x.ShowTimeID == showTimeId).SelectMany(x => x.SeatNames).ToList(); }

        public static Booking AddBooking(int userId, int showTimeId, IEnumerable<string> seats, decimal total, string method)
        {
            lock (SyncRoot)
            {
                var booking = new Booking { BookingID = Bookings.Count + 1001, UserID = userId, ShowTimeID = showTimeId, SeatNames = seats.ToList(), TotalMoney = total, BookingDate = DateTime.Now, PaymentMethod = method, Status = "Đã thanh toán" };
                booking.Code = "PHP" + booking.BookingDate.ToString("yyMMdd") + booking.BookingID; Bookings.Add(booking); return booking;
            }
        }

        public static List<Booking> GetBookings(int userId) { lock (SyncRoot) return Bookings.Where(x => x.UserID == userId).OrderByDescending(x => x.BookingDate).ToList(); }

        private static Movy Movie(int id, string title, string genre, int duration, string director, string actors, string description, decimal rating, string age, bool hot, bool early)
        {
            var upcoming = id >= 13;
            return new Movy
            {
                MovieID = id, Title = title, Genre = genre, Duration = duration, Director = director, Actors = actors, Description = description,
                Accent = new[] { "#ff7a45", "#e94f64", "#f1b93a", "#4fa6a2", "#6d8f62", "#8c63d9" }[(id - 1) % 6],
                Rating = rating, AgeLimit = age, Language = "Tiếng Việt / Phụ đề", ReleaseDate = DateTime.Today.AddDays(upcoming ? id - 8 : id - 10),
                Status = upcoming ? "Sắp chiếu" : "Đang chiếu", Poster = "movie" + id + ".jpg", Country = id % 3 == 0 ? "Việt Nam" : "Mỹ",
                Format = id % 2 == 0 ? "2D, IMAX" : "2D, Dolby Atmos", IsHot = hot, IsEarlyAccess = early, Cast = BuildCast(id, actors)
            };
        }

        private static List<CastMember> BuildCast(int movieId, string actors)
        {
            return (actors ?? "").Split(',').Select((name, index) => new CastMember
            {
                Name = name.Trim(), Character = index == 0 ? "Vai chính" : "Vai phụ",
                Image = "movie" + (((movieId + index + 7) % 20) + 1) + ".jpg"
            }).ToList();
        }

        private static Event Event(int id, string title, string label, string description, string image, string accent, int days)
        {
            return new Event { EventID = id, Title = title, Label = label, Description = description, Image = image, Accent = accent, EndDate = DateTime.Today.AddDays(days) };
        }

        private static User CloneUser(User x) { return new User { UserID = x.UserID, UserName = x.UserName, FullName = x.FullName, Email = x.Email, Phone = x.Phone, Role = x.Role }; }
        private static string HashPassword(string value) { using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""))); }
    }
}
