using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Bus
{
    public int BusId { get; set; }

    public string? BusCode { get; set; }

    public string? BusNumber { get; set; }

    public string? BusType { get; set; }

    public int TotalSeats { get; set; }

    public string? StartingPoint { get; set; }

    public string? DestinationPoint { get; set; }

    public string? RouteDescription { get; set; }

    public TimeOnly? DepartureTime { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public int DistanceInKm { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
