using System;
using System.Collections.Generic;

namespace MovieTicketDB.Models
{
    public class Movy
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Poster { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public string Language { get; set; }
        public string AgeLimit { get; set; }
        public decimal Rating { get; set; }
        public string Accent { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public string Format { get; set; }
        public bool IsHot { get; set; }
        public bool IsEarlyAccess { get; set; }
        public List<CastMember> Cast { get; set; }
    }

    public class CastMember
    {
        public string Name { get; set; }
        public string Character { get; set; }
        public string Image { get; set; }
    }

    public class Cinema
    {
        public int CinemaID { get; set; }
        public string CinemaName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Accent { get; set; }
    }

    public class Room
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public int CinemaID { get; set; }
        public Cinema Cinema { get; set; }
    }

    public class ShowTime
    {
        public int ShowTimeID { get; set; }
        public int MovieID { get; set; }
        public int RoomID { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal Price { get; set; }
        public Movy Movy { get; set; }
        public Room Room { get; set; }
    }

    public class Seat
    {
        public int SeatID { get; set; }
        public string SeatName { get; set; }
        public int RoomID { get; set; }
        public string Type { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
    }

    public class Event
    {
        public int EventID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Label { get; set; }
        public string Accent { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ConcessionProduct
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Accent { get; set; }
        public string ShortCode { get; set; }
        public string Image { get; set; }
    }

    public class ConcessionOrderItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get { return UnitPrice * Quantity; } }
    }

    public class Booking
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public int ShowTimeID { get; set; }
        public List<string> SeatNames { get; set; }
        public decimal TotalMoney { get; set; }
        public DateTime BookingDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public decimal TicketMoney { get; set; }
        public decimal ConcessionMoney { get; set; }
        public List<ConcessionOrderItem> Concessions { get; set; }
    }

    public class StoreOrder
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public List<ConcessionOrderItem> Items { get; set; }
        public decimal TotalMoney { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
    }
}
