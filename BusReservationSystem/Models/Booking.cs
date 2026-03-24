using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public DateTime? BookingDate { get; set; }

    public DateOnly TravelDate { get; set; }

    public int? BusId { get; set; }

    public int? CustomerId { get; set; }

    public int? SeatNumber { get; set; }

    public int? BookedBy { get; set; }

    public string? StartingPoint { get; set; }

    public string? DestinationPoint { get; set; }

    public decimal? BaseFare { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public string? BookingStatus { get; set; }

    public DateTime? CancellationDate { get; set; }

    public decimal? RefundAmount { get; set; }

    public virtual Employee? BookedByNavigation { get; set; }

    public virtual Bus? Bus { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
