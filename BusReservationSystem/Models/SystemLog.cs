using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class SystemLog
{
    public int Id { get; set; }

    public string? AdminEmail { get; set; }

    public string? Action { get; set; }

    public string? Module { get; set; }

    public string? Details { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? IpAddress { get; set; }
}
