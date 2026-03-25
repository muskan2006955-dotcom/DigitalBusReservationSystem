using System;
using System.Collections.Generic;

namespace BusReservationSystem.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public int? Age { get; set; }

    public string? Qualification { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? BranchLocation { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public string? ProfilePicture { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
