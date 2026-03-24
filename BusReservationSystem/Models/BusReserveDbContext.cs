using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Models;

public partial class BusReserveDbContext : DbContext
{
    public BusReserveDbContext()
    {
    }

    public BusReserveDbContext(DbContextOptions<BusReserveDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<CancellationPolicy> CancellationPolicies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<PriceList> PriceLists { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-NJN80PI\\SQLEXPRESS;Database=BusReservationDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951ACD3350D949");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BaseFare).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Booked");
            entity.Property(e => e.BusId).HasColumnName("BusID");
            entity.Property(e => e.CancellationDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DestinationPoint)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StartingPoint)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.BookedByNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BookedBy)
                .HasConstraintName("FK__Booking__BookedB__6383C8BA");

            entity.HasOne(d => d.Bus).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BusId)
                .HasConstraintName("FK__Booking__BusID__619B8048");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Booking__Custome__628FA481");
        });

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.BusId).HasName("PK__Bus__6A0F6095F25C1290");

            entity.ToTable("Bus");

            entity.HasIndex(e => e.BusCode, "UQ__Bus__5C50D802117E6E21").IsUnique();

            entity.Property(e => e.BusId).HasColumnName("BusID");
            entity.Property(e => e.BusCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.BusNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BusType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DestinationPoint)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DistanceInKm).HasColumnName("DistanceInKM");
            entity.Property(e => e.RouteDescription)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.StartingPoint)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<CancellationPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Cancella__2E1339448068662C");

            entity.ToTable("CancellationPolicy");

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.DeductionPercentage).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8EF167567");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.IdproofNumber, "UQ_IdproofNumber").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IdproofNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDProofNumber");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF12DA2362E");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.Username, "UQ__Employee__536C85E42B57C5F2").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.BranchLocation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__PriceLis__4957584FE16D1348");

            entity.ToTable("PriceList");

            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.BusType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PricePerKm)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PricePerKM");
            entity.Property(e => e.TaxPercentage).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__Seat__311713D3938B8E0F");

            entity.ToTable("Seat");

            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.BusId).HasColumnName("BusID");
            entity.Property(e => e.SeatStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Bus).WithMany(p => p.Seats)
                .HasForeignKey(d => d.BusId)
                .HasConstraintName("FK__Seat__BusID__5535A963");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemLo__3214EC070B0EEDC6");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.AdminEmail).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC607BD84A565");

            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CancellationDate).HasColumnType("datetime");
            entity.Property(e => e.CancellationDeduction).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PassengerName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Booked");

            entity.HasOne(d => d.Booking).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Tickets_Booking");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
