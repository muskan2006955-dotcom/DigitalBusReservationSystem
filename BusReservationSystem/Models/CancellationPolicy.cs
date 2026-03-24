using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class CancellationPolicy
{
    public int PolicyId { get; set; }

    public int? MinimumDaysBeforeTravel { get; set; }

    public decimal? DeductionPercentage { get; set; }
}
