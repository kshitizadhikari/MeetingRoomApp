using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoomApp.Models;
using System.Reflection.Emit;

namespace RoomApp.DataAccess.DAL;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Room>()
                .HasMany(r => r.Bookings)
                .WithOne(b => b.Room)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Booking>()
                .HasMany(b => b.Participants)
                .WithOne(p => p.Booking)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<ApplicationUser>()
               .HasMany(u => u.Bookings)
               .WithOne(b => b.User)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        //unique room and booking names
        builder.Entity<Room>()
            .HasIndex(r => r.Name)
            .IsUnique();

        builder.Entity<Booking>()
            .HasIndex(b => b.Name)
        .IsUnique();

        builder.Entity<Room>().HasQueryFilter(r => !r.isDeleted);
        builder.Entity<Booking>().HasQueryFilter(r => !r.isDeleted);
        builder.Entity<Participant>().HasQueryFilter(r => !r.isDeleted);

        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Room> Rooms { get; set; }
}
