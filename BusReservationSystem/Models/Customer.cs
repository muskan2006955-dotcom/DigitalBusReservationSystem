using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public int Age { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? IdproofNumber { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? RegisteredBy { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
