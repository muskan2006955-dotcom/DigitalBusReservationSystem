using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class PriceList
{
    public int PriceId { get; set; }

    public string? BusType { get; set; }

    public decimal? PricePerKm { get; set; }

    public decimal? TaxPercentage { get; set; }

    public DateTime? EffectiveDate { get; set; }
}
