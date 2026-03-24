using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public int? BusId { get; set; }

    public int? SeatNumber { get; set; }

    public string? SeatStatus { get; set; }

    public virtual Bus? Bus { get; set; }
}
