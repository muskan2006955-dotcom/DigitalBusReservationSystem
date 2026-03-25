using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int BusId { get; set; }

    public int EmployeeId { get; set; }

    public string PassengerName { get; set; } = null!;

    public DateOnly TravelDate { get; set; }

    public DateTime BookingDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CancellationDate { get; set; }

    public decimal? CancellationDeduction { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }
}
