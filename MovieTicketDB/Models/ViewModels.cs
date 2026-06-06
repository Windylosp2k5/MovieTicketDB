using System.Collections.Generic;

using PagedList;

namespace MovieTicketDB.Models
{
    public class MovieIndexViewModel
    {
        public IPagedList<Movy> Movies { get; set; }
        public IEnumerable<Movy> FeaturedMovies { get; set; }
        public string Search { get; set; }
        public string Genre { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public IEnumerable<Movy> HotMovies { get; set; }
        public IEnumerable<Movy> UpcomingMovies { get; set; }
        public IEnumerable<Movy> EarlyMovies { get; set; }
        public IEnumerable<Event> Events { get; set; }
    }

    public class SeatSelectionViewModel
    {
        public ShowTime ShowTime { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
        public IEnumerable<string> BookedSeats { get; set; }
    }

    public class PaymentViewModel
    {
        public ShowTime ShowTime { get; set; }
        public List<string> SeatNames { get; set; }
        public decimal TicketMoney { get; set; }
        public decimal ConcessionMoney { get; set; }
        public List<ConcessionOrderItem> Concessions { get; set; }
        public decimal TotalMoney { get; set; }
        public string PaymentCode { get; set; }
        public string QrCodeBase64 { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public IEnumerable<PaymentQrOption> QrOptions { get; set; }
    }

    public class StoreViewModel
    {
        public ShowTime ShowTime { get; set; }
        public List<string> SeatNames { get; set; }
        public decimal TicketMoney { get; set; }
        public IEnumerable<ConcessionProduct> Products { get; set; }
    }

    public class StorePaymentViewModel
    {
        public List<ConcessionOrderItem> Items { get; set; }
        public decimal TotalMoney { get; set; }
        public string PaymentCode { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public IEnumerable<PaymentQrOption> QrOptions { get; set; }
    }

    public class StoreOrderReceiptViewModel
    {
        public StoreOrder Order { get; set; }
        public User User { get; set; }
    }

    public class PaymentQrOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Receiver { get; set; }
        public string Account { get; set; }
        public string QrCodeBase64 { get; set; }
        public string Accent { get; set; }
    }

    public class TicketReceiptViewModel
    {
        public Booking Booking { get; set; }
        public ShowTime ShowTime { get; set; }
        public User User { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public IEnumerable<Movy> Movies { get; set; }
        public IEnumerable<ShowTime> ShowTimes { get; set; }
        public IEnumerable<Event> Events { get; set; }
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Booking> Bookings { get; set; }
        public IEnumerable<Cinema> Cinemas { get; set; }
        public decimal Revenue { get; set; }
        public int TotalShowTimes { get; set; }
    }
}
