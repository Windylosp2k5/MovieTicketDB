using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace MovieTicketDB.Models
{
    public static class CinemaStore
    {
        private static readonly object SyncRoot = new object();
        private static bool IsInitializing = true;

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
        public static readonly List<ConcessionProduct> Concessions = new List<ConcessionProduct>
        {
            Product(1, "Bắp rang bơ nhỏ", "Popcorn", "Phần bắp bơ nhỏ, giòn thơm.", 35000, "#f5bd3c", "popcorn_butter_s.jpg"),
            Product(2, "Bắp rang bơ vừa", "Popcorn", "Phần bắp bơ vừa cho một người.", 45000, "#efad2f", "popcorn_butter_s.jpg"),
            Product(3, "Bắp rang bơ lớn", "Popcorn", "Phần bắp bơ lớn để chia sẻ.", 55000, "#e69a24", "popcorn_butter_s.jpg"),
            Product(4, "Bắp rang phô mai", "Popcorn", "Bắp rang phủ phô mai đậm vị.", 60000, "#f08b32", "popcorn_cheese.jpg"),
            Product(5, "Bắp caramel", "Popcorn", "Caramel ngọt dịu phủ đều từng hạt.", 65000, "#b8793f", "popcorn_caramel.jpg"),

            Product(6, "Coca Cola nhỏ", "Drink", "Ly Coca Cola nhỏ dùng lạnh.", 25000, "#e32636", "coca_s.jpg"),
            Product(7, "Coca Cola lớn", "Drink", "Ly Coca Cola lớn dùng lạnh.", 35000, "#c7192a", "coca_l.jpg"),
            Product(8, "Pepsi", "Drink", "Pepsi mát lạnh cho suất chiếu.", 30000, "#1769d2", "pepsi.jpg"),
            Product(9, "7 Up", "Drink", "Nước ngọt vị chanh thanh mát.", 30000, "#34a853", "7up.jpg"),
            Product(10, "Trà đào", "Drink", "Trà đào thơm dịu, dùng lạnh.", 35000, "#e78945", "peach_tea.jpg"),
            Product(11, "Nước suối", "Drink", "Nước suối tinh khiết.", 20000, "#4e9ccb", "water.jpg"),

            Product(12, "Combo Solo", "Combo", "Combo gọn nhẹ dành cho một người.", 70000, "#e85d3f", "combo_solo.jpg"),
            Product(13, "Combo Couple", "Combo", "Combo bắp nước dành cho hai người.", 110000, "#d14f77", "combo_couple.jpg"),
            Product(14, "Combo Family", "Combo", "Combo tiết kiệm dành cho gia đình.", 220000, "#7658d8", "combo_family.jpg"),
            Product(15, "Combo VIP", "Combo", "Combo cao cấp cho trải nghiệm trọn vẹn.", 140000, "#b78b39", "combo_solo.jpg"),
            Product(16, "Combo Kids", "Combo", "Combo nhỏ xinh dành cho trẻ em.", 55000, "#35a8a0", "combo_kids.jpg"),
            Product(17, "Combo Sweet", "Combo", "Combo ngọt ngào dành cho buổi hẹn.", 95000, "#dc6788", "combo_sweet.jpg")
        };
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
        private static readonly List<StoreOrder> StoreOrders = new List<StoreOrder>();

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
            var stateVersion = LoadState();
            if (stateVersion < 2) ApplyMovieCatalogV2();
            IsInitializing = false;
            SaveState();
        }

        public static bool RebuildShowTimes()
        {
            lock (SyncRoot)
            {
                if (!IsInitializing && Bookings.Any()) return false;
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
                SaveStateUnsafe();
                return true;
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
                user.UserID = Users.Max(x => x.UserID) + 1; user.Password = HashPassword(user.Password); user.Role = "Customer"; Users.Add(user); SaveStateUnsafe(); return true;
            }
        }

        public static bool SetUserRole(int id, string role)
        {
            lock (SyncRoot)
            {
                var user = Users.FirstOrDefault(x => x.UserID == id);
                if (user == null || user.UserName == "admin") return false;
                user.Role = role == "Admin" ? "Admin" : "Customer";
                SaveStateUnsafe();
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
                    input.Poster = string.IsNullOrWhiteSpace(input.Poster) ? "film1.jpg" : input.Poster;
                    input.Cast = BuildCast(input.MovieID, input.Actors);
                    Movies.Add(input);
                    SaveStateUnsafe();
                    return input;
                }
                movie.Title = input.Title; movie.Genre = input.Genre; movie.Duration = input.Duration; movie.Director = input.Director;
                movie.Actors = input.Actors; movie.Description = input.Description; movie.Language = input.Language; movie.AgeLimit = input.AgeLimit; movie.Rating = input.Rating;
                movie.Status = input.Status; movie.Poster = input.Poster; movie.ReleaseDate = input.ReleaseDate; movie.Country = input.Country;
                movie.Format = input.Format; movie.IsHot = input.IsHot; movie.IsEarlyAccess = input.IsEarlyAccess; movie.Cast = BuildCast(movie.MovieID, movie.Actors);
                SaveStateUnsafe();
                return movie;
            }
        }

        public static bool DeleteMovie(int id)
        {
            lock (SyncRoot)
            {
                var movie = Movies.FirstOrDefault(x => x.MovieID == id);
                if (movie == null) return false;
                if (ShowTimes.Any(x => x.MovieID == id)) return false;
                Movies.Remove(movie); SaveStateUnsafe(); return true;
            }
        }

        public static string GetMovieDeleteBlockReason(int id)
        {
            lock (SyncRoot)
            {
                var showTimeIds = ShowTimes.Where(x => x.MovieID == id).Select(x => x.ShowTimeID).ToList();
                if (Bookings.Any(x => showTimeIds.Contains(x.ShowTimeID)))
                    return "Không thể xóa phim vì đã có vé được đặt. Hãy chuyển phim sang trạng thái Ngừng chiếu để giữ lịch sử giao dịch.";
                if (showTimeIds.Any())
                    return "Không thể xóa phim vì vẫn còn suất chiếu. Hãy xóa các suất chiếu chưa có vé trước.";
                return null;
            }
        }

        public static Event SaveEvent(Event input)
        {
            lock (SyncRoot)
            {
                var item = Events.FirstOrDefault(x => x.EventID == input.EventID);
                if (item == null) { input.EventID = Events.Any() ? Events.Max(x => x.EventID) + 1 : 1; Events.Add(input); SaveStateUnsafe(); return input; }
                item.Title = input.Title; item.Label = input.Label; item.Description = input.Description; item.Image = input.Image;
                item.Accent = input.Accent; item.EndDate = input.EndDate; SaveStateUnsafe(); return item;
            }
        }

        public static bool DeleteEvent(int id) { lock (SyncRoot) { var item = Events.FirstOrDefault(x => x.EventID == id); if (item == null) return false; Events.Remove(item); SaveStateUnsafe(); return true; } }
        public static Cinema SaveCinema(Cinema input)
        {
            lock (SyncRoot)
            {
                var item = Cinemas.FirstOrDefault(x => x.CinemaID == input.CinemaID);
                if (item != null)
                {
                    item.CinemaName = input.CinemaName; item.Address = input.Address; item.Description = input.Description; item.Accent = input.Accent;
                    SaveStateUnsafe();
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
                SaveStateUnsafe();
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
                if (Bookings.Any(x => ShowTimes.Any(s => s.ShowTimeID == x.ShowTimeID && roomIds.Contains(s.RoomID)))) return false;
                ShowTimes.RemoveAll(x => roomIds.Contains(x.RoomID)); Seats.RemoveAll(x => roomIds.Contains(x.RoomID)); Rooms.RemoveAll(x => roomIds.Contains(x.RoomID)); Cinemas.Remove(item); SaveStateUnsafe();
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
                    input.Movy = movie; input.Room = room; ShowTimes.Add(input); SaveStateUnsafe(); return input;
                }
                item.MovieID = input.MovieID; item.RoomID = input.RoomID; item.Movy = movie; item.Room = room;
                item.ShowDate = input.ShowDate; item.StartTime = input.StartTime; item.Price = input.Price; SaveStateUnsafe(); return item;
            }
        }
        public static bool DeleteShowTime(int id) { lock (SyncRoot) { var item = ShowTimes.FirstOrDefault(x => x.ShowTimeID == id); if (item == null || Bookings.Any(x => x.ShowTimeID == id)) return false; ShowTimes.Remove(item); SaveStateUnsafe(); return true; } }
        public static bool SetBookingStatus(int id, string status)
        {
            lock (SyncRoot)
            {
                var item = Bookings.FirstOrDefault(x => x.BookingID == id);
                if (item == null) return false;
                item.Status = new[] { "Đã thanh toán", "Đã sử dụng", "Đã hủy" }.Contains(status) ? status : item.Status;
                SaveStateUnsafe();
                return true;
            }
        }
        public static IEnumerable<string> GetBookedSeats(int showTimeId) { lock (SyncRoot) return Bookings.Where(x => x.ShowTimeID == showTimeId).SelectMany(x => x.SeatNames).ToList(); }

        public static Booking AddBooking(int userId, int showTimeId, IEnumerable<string> seats, decimal ticketMoney, IEnumerable<ConcessionOrderItem> concessions, string method)
        {
            lock (SyncRoot)
            {
                var orderedItems = (concessions ?? Enumerable.Empty<ConcessionOrderItem>()).ToList();
                var concessionMoney = orderedItems.Sum(x => x.TotalPrice);
                var booking = new Booking { BookingID = Bookings.Count + 1001, UserID = userId, ShowTimeID = showTimeId, SeatNames = seats.ToList(), TicketMoney = ticketMoney, ConcessionMoney = concessionMoney, Concessions = orderedItems, TotalMoney = ticketMoney + concessionMoney, BookingDate = DateTime.Now, PaymentMethod = method, Status = "Đã thanh toán" };
                booking.Code = "PHP" + booking.BookingDate.ToString("yyMMdd") + booking.BookingID; Bookings.Add(booking); SaveStateUnsafe(); return booking;
            }
        }

        public static List<Booking> GetBookings(int userId) { lock (SyncRoot) return Bookings.Where(x => x.UserID == userId).OrderByDescending(x => x.BookingDate).ToList(); }

        public static StoreOrder AddStoreOrder(int userId, IEnumerable<ConcessionOrderItem> items, string method)
        {
            lock (SyncRoot)
            {
                var orderedItems = (items ?? Enumerable.Empty<ConcessionOrderItem>()).ToList();
                var order = new StoreOrder
                {
                    OrderID = StoreOrders.Count + 5001,
                    UserID = userId,
                    Items = orderedItems,
                    TotalMoney = orderedItems.Sum(x => x.TotalPrice),
                    OrderDate = DateTime.Now,
                    PaymentMethod = method,
                    Status = "Đã thanh toán"
                };
                order.Code = "SHOP" + order.OrderDate.ToString("yyMMdd") + order.OrderID;
                StoreOrders.Add(order);
                SaveStateUnsafe();
                return order;
            }
        }

        public static List<StoreOrder> GetStoreOrders(int userId)
        {
            lock (SyncRoot) return StoreOrders.Where(x => x.UserID == userId).OrderByDescending(x => x.OrderDate).ToList();
        }

        private static void ApplyMovieCatalogV2()
        {
            var catalog = new[]
            {
                CatalogMovie(1, "Avengers: Endgame", "Biệt đội siêu anh hùng chiến đấu bảo vệ vũ trụ.", 181, "Hành động", 2019, 4, 26, "film1.jpg", "Anthony Russo", "Robert Downey Jr, Chris Evans, Scarlett Johansson", "Tiếng Anh - Phụ đề Việt", "C13 - Khán giả từ 13 tuổi", 9.0m),
                CatalogMovie(2, "Doraemon: Nobita và vùng đất lý tưởng", "Phim hoạt hình Doraemon mới.", 110, "Hoạt hình", 2023, 5, 26, "film2.jpg", "Tetsuo Yajima", "Wasabi Mizuta, Megumi Ohara, Yumi Kakazu", "Tiếng Nhật - Lồng tiếng Việt", "P - Mọi lứa tuổi", 8.5m),
                CatalogMovie(3, "Mai", "Phim tâm lý tình cảm Việt Nam.", 130, "Tình cảm", 2024, 2, 10, "film3.jpg", "Trấn Thành", "Phương Anh Đào, Tuấn Trần", "Tiếng Việt", "C16 - Khán giả từ 16 tuổi", 8.2m),
                CatalogMovie(4, "Fast & Furious 10", "Biệt đội đua xe tốc độ đối đầu thế lực nguy hiểm.", 141, "Hành động", 2023, 5, 19, "film4.jpg", "Louis Leterrier", "Vin Diesel, Jason Momoa", "Tiếng Anh", "C16 - Khán giả từ 16 tuổi", 7.8m),
                CatalogMovie(5, "John Wick 4", "Sát thủ John Wick trở lại với cuộc chiến sinh tử.", 169, "Hành động", 2023, 3, 24, "film5.jpg", "Nguyễn Quang Dũng", "Kaity Nguyễn, Thanh Sơn", "Tiếng Việt", "C13 - Khán giả từ 13 tuổi", 8.0m),
                CatalogMovie(6, "Spider-Man: No Way Home", "Người Nhện đối mặt đa vũ trụ.", 148, "Siêu anh hùng", 2021, 12, 17, "film6.jpg", "Jon Watts", "Tom Holland, Zendaya", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 9.1m),
                CatalogMovie(7, "Avatar: The Way of Water", "Gia đình Jake Sully trên hành tinh Pandora.", 192, "Khoa học viễn tưởng", 2022, 12, 16, "film7.jpg", "James Cameron", "Sam Worthington, Zoe Saldana", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 9.4m),
                CatalogMovie(8, "Inside Out 2", "Câu chuyện cảm xúc mới của Riley.", 100, "Hoạt hình", 2024, 6, 14, "film8.jpg", "Kelsey Mann", "Amy Poehler, Maya Hawke", "Tiếng Anh - Lồng tiếng Việt", "P - Mọi lứa tuổi", 8.9m),
                CatalogMovie(9, "Kung Fu Panda 4", "Gấu Po trở lại với hành trình mới.", 94, "Hoạt hình", 2024, 3, 8, "film9.jpg", "Michael Bay", "Mark Wahlberg, Nicola Peltz", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 7.9m),
                CatalogMovie(10, "Frozen 2", "Elsa khám phá bí mật sức mạnh của mình.", 103, "Hoạt hình", 2019, 11, 22, "film10.jpg", "Christopher Nolan", "Cillian Murphy, Emily Blunt", "Tiếng Anh", "C16 - Khán giả từ 16 tuổi", 9.3m),
                CatalogMovie(11, "Black Panther", "Vua TChalla bảo vệ Wakanda.", 134, "Hành động", 2018, 2, 16, "film11.jpg", "Greta Gerwig", "Margot Robbie, Ryan Gosling", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 8.1m),
                CatalogMovie(12, "The Batman", "Batman điều tra những vụ án bí ẩn tại Gotham.", 176, "Hành động", 2022, 3, 4, "film12.jpg", "Hayao Miyazaki", "Soma Santoki, Masaki Suda", "Tiếng Nhật", "P - Mọi lứa tuổi", 9.5m),
                CatalogMovie(13, "Interstellar", "Hành trình khám phá không gian để cứu loài người.", 169, "Khoa học viễn tưởng", 2014, 11, 7, "film13.jpg", "Matt Reeves", "Robert Pattinson, Zoe Kravitz", "Tiếng Anh", "C16 - Khán giả từ 16 tuổi", 8.7m),
                CatalogMovie(14, "Your Name", "Câu chuyện hoán đổi cơ thể đầy cảm xúc.", 106, "Anime", 2016, 8, 26, "film14.jpg", "Denis Villeneuve", "Timothee Chalamet, Zendaya", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 9.2m),
                CatalogMovie(15, "Detective Conan Movie", "Conan phá giải vụ án nguy hiểm.", 110, "Anime", 2023, 4, 14, "film15.jpg", "Ridley Scott", "Paul Mescal, Pedro Pascal", "Tiếng Anh", "C18 - Khán giả từ 18 tuổi", 8.4m),
                CatalogMovie(16, "One Piece Film Red", "Luffy và đồng đội gặp ca sĩ Uta.", 115, "Anime", 2022, 8, 6, "film16.jpg", "David Yates", "Daniel Radcliffe, Emma Watson", "Tiếng Anh", "P - Mọi lứa tuổi", 9.0m),
                CatalogMovie(17, "Train To Busan", "Cuộc chiến sinh tồn trên chuyến tàu zombie.", 118, "Kinh dị", 2016, 7, 20, "film17.jpg", "Patty Jenkins", "Gal Gadot, Chris Pine", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 8.0m),
                CatalogMovie(18, "The Nun", "Ác quỷ ma sơ gieo rắc nỗi kinh hoàng.", 96, "Kinh dị", 2018, 9, 7, "film18.jpg", "Chad Stahelski", "Keanu Reeves, Donnie Yen", "Tiếng Anh", "C18 - Khán giả từ 18 tuổi", 9.1m),
                CatalogMovie(19, "Annabelle Comes Home", "Búp bê ma ám quay trở lại.", 106, "Kinh dị", 2019, 6, 26, "film19.jpg", "Peter Jackson", "Elijah Wood, Ian McKellen", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 9.6m),
                CatalogMovie(20, "La La Land", "Chuyện tình lãng mạn giữa nghệ sĩ piano và diễn viên.", 128, "Tình cảm", 2016, 12, 9, "film20.jpg", "J.A. Bayona", "Tom Holland, Naomi Watts", "Tiếng Anh", "C13 - Khán giả từ 13 tuổi", 8.3m)
            };

            foreach (var source in catalog)
            {
                var movie = Movies.FirstOrDefault(x => x.MovieID == source.MovieID);
                if (movie == null)
                {
                    Movies.Add(source);
                    continue;
                }

                movie.Title = source.Title;
                movie.Description = source.Description;
                movie.Duration = source.Duration;
                movie.Genre = source.Genre;
                movie.ReleaseDate = source.ReleaseDate;
                movie.Poster = source.Poster;
                movie.Director = source.Director;
                movie.Actors = source.Actors;
                movie.Language = source.Language;
                movie.AgeLimit = source.AgeLimit;
                movie.Rating = source.Rating;
                movie.Cast = source.Cast;
            }

            ReconnectReferences();
        }

        private static Movy CatalogMovie(int id, string title, string description, int duration, string genre, int year, int month, int day, string poster, string director, string actors, string language, string ageLimit, decimal rating)
        {
            return new Movy
            {
                MovieID = id,
                Title = title,
                Description = description,
                Duration = duration,
                Genre = genre,
                ReleaseDate = new DateTime(year, month, day),
                Poster = poster,
                Director = director,
                Actors = actors,
                Language = language,
                AgeLimit = ageLimit,
                Rating = rating,
                Accent = new[] { "#ff7a45", "#e94f64", "#f1b93a", "#4fa6a2", "#6d8f62", "#8c63d9" }[(id - 1) % 6],
                Status = "Đang chiếu",
                Country = language == "Tiếng Việt" ? "Việt Nam" : "Quốc tế",
                Format = id % 2 == 0 ? "2D, IMAX" : "2D, Dolby Atmos",
                IsHot = rating >= 8.8m,
                IsEarlyAccess = false,
                Cast = BuildCast(id, actors)
            };
        }

        private static Movy Movie(int id, string title, string genre, int duration, string director, string actors, string description, decimal rating, string age, bool hot, bool early)
        {
            var upcoming = id >= 13;
            return new Movy
            {
                MovieID = id, Title = title, Genre = genre, Duration = duration, Director = director, Actors = actors, Description = description,
                Accent = new[] { "#ff7a45", "#e94f64", "#f1b93a", "#4fa6a2", "#6d8f62", "#8c63d9" }[(id - 1) % 6],
                Rating = rating, AgeLimit = age, Language = "Tiếng Việt / Phụ đề", ReleaseDate = DateTime.Today.AddDays(upcoming ? id - 8 : id - 10),
                Status = upcoming ? "Sắp chiếu" : "Đang chiếu", Poster = "film" + id + ".jpg", Country = id % 3 == 0 ? "Việt Nam" : "Mỹ",
                Format = id % 2 == 0 ? "2D, IMAX" : "2D, Dolby Atmos", IsHot = hot, IsEarlyAccess = early, Cast = BuildCast(id, actors)
            };
        }

        private static List<CastMember> BuildCast(int movieId, string actors)
        {
            return (actors ?? "").Split(',').Select((name, index) => new CastMember
            {
                Name = name.Trim(), Character = index == 0 ? "Vai chính" : "Vai phụ",
                Image = "film" + (((movieId + index + 7) % 20) + 1) + ".jpg"
            }).ToList();
        }

        private static Event Event(int id, string title, string label, string description, string image, string accent, int days)
        {
            return new Event { EventID = id, Title = title, Label = label, Description = description, Image = image, Accent = accent, EndDate = DateTime.Today.AddDays(days) };
        }

        private static ConcessionProduct Product(int id, string name, string category, string description, decimal price, string accent, string image)
        {
            return new ConcessionProduct { ProductID = id, Name = name, Category = category, Description = description, BasePrice = price, Accent = accent, Image = image };
        }

        private static User CloneUser(User x) { return new User { UserID = x.UserID, UserName = x.UserName, FullName = x.FullName, Email = x.Email, Phone = x.Phone, Role = x.Role }; }
        private static string HashPassword(string value) { using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""))); }

        private static string StatePath
        {
            get
            {
                var path = HostingEnvironment.MapPath("~/App_Data");
                if (string.IsNullOrWhiteSpace(path)) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                Directory.CreateDirectory(path);
                return Path.Combine(path, "cinema-state.json");
            }
        }

        private static void SaveState()
        {
            lock (SyncRoot) SaveStateUnsafe();
        }

        private static void SaveStateUnsafe()
        {
            if (IsInitializing) return;
            var state = new CinemaState
            {
                Version = 2,
                Movies = Movies,
                Cinemas = Cinemas,
                Rooms = Rooms,
                ShowTimes = ShowTimes,
                Seats = Seats,
                Events = Events,
                Users = Users,
                Bookings = Bookings,
                StoreOrders = StoreOrders
            };
            var json = JsonConvert.SerializeObject(state, Formatting.Indented);
            var tempPath = StatePath + ".tmp";
            File.WriteAllText(tempPath, json, new UTF8Encoding(false));
            if (File.Exists(StatePath)) File.Replace(tempPath, StatePath, null);
            else File.Move(tempPath, StatePath);
        }

        private static int LoadState()
        {
            try
            {
                if (!File.Exists(StatePath)) return 0;
                var json = File.ReadAllText(StatePath, Encoding.UTF8);
                var state = JsonConvert.DeserializeObject<CinemaState>(json);
                if (state == null || state.Movies == null || !state.Movies.Any()) return 0;
                Replace(Movies, state.Movies);
                Replace(Cinemas, state.Cinemas);
                Replace(Rooms, state.Rooms);
                Replace(ShowTimes, state.ShowTimes);
                Replace(Seats, state.Seats);
                Replace(Events, state.Events);
                Replace(Users, state.Users);
                Replace(Bookings, state.Bookings);
                Replace(StoreOrders, state.StoreOrders);
                ReconnectReferences();
                return state.Version;
            }
            catch
            {
                // Keep the built-in seed data when a state file is damaged.
                return 0;
            }
        }

        private static void ReconnectReferences()
        {
            foreach (var room in Rooms)
                room.Cinema = Cinemas.FirstOrDefault(x => x.CinemaID == room.CinemaID);
            foreach (var showTime in ShowTimes)
            {
                showTime.Movy = Movies.FirstOrDefault(x => x.MovieID == showTime.MovieID);
                showTime.Room = Rooms.FirstOrDefault(x => x.RoomID == showTime.RoomID);
            }
            foreach (var movie in Movies)
                movie.Cast = BuildCast(movie.MovieID, movie.Actors);
        }

        private static void Replace<T>(List<T> target, IEnumerable<T> source)
        {
            target.Clear();
            if (source != null) target.AddRange(source);
        }

        private class CinemaState
        {
            public int Version { get; set; }
            public List<Movy> Movies { get; set; }
            public List<Cinema> Cinemas { get; set; }
            public List<Room> Rooms { get; set; }
            public List<ShowTime> ShowTimes { get; set; }
            public List<Seat> Seats { get; set; }
            public List<Event> Events { get; set; }
            public List<User> Users { get; set; }
            public List<Booking> Bookings { get; set; }
            public List<StoreOrder> StoreOrders { get; set; }
        }
    }
}
