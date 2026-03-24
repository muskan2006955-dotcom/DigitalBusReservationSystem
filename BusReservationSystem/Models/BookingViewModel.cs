using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models
{
    public class BookingViewModel
    {
        public List<Bus> Buses { get; set; }
        public List<PriceList> PriceRules { get; set; }
        public Bus SelectedBus { get; set; }
        public Customer Customer { get; set; }
        public int? SeatId { get; set; }
        public DateOnly TravelDate { get; set; }
        public decimal? BaseFare { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? FinalAmount { get; set; }
    }
}