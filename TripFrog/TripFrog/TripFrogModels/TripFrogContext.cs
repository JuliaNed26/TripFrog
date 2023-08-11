using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TripFrogModels.Models;

namespace TripFrogModels;

public sealed class TripFrogContext : DbContext
{
    public TripFrogContext(DbContextOptions<TripFrogContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Destination> Destinations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureRelationships(modelBuilder);

        modelBuilder.Entity<Trip>()
            .HasMany(trip => trip.Bookings)
            .WithOne(booking => booking.Trip)
            .OnDelete(DeleteBehavior.ClientCascade);

        modelBuilder.Entity<Apartment>()
            .Property(apartment => apartment.PricePerDay)
            .HasColumnType("decimal(20, 2)");

        modelBuilder.Entity<Place>()
            .Property(place => place.Price)
            .HasColumnType("decimal(20, 2)");

        modelBuilder.Entity<User>()
            .Property(user => user.Email)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<LanguageUser>().HasKey(langUser => new { langUser.UserId, langUser.LanguageId });
    }
    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LanguageUser>()
            .HasOne(langUser => langUser.User)
            .WithMany(user => user.LanguageUsers)
            .HasForeignKey(langUser => langUser.UserId);

        modelBuilder.Entity<LanguageUser>()
            .HasOne(langUser => langUser.Language)
            .WithMany(language => language.LanguageUsers)
            .HasForeignKey(langUser => langUser.LanguageId);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Trips)
            .WithOne(trip => trip.User)
            .HasForeignKey(trip => trip.UserId);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Apartments)
            .WithOne(apartment => apartment.User)
            .HasForeignKey(apartment => apartment.UserId);

        modelBuilder.Entity<Apartment>()
            .HasMany(apartment => apartment.ApartmentPhotos)
            .WithOne(photo => photo.Apartment)
            .HasForeignKey(photo => photo.ApartmentId);

        modelBuilder.Entity<Booking>()
            .HasOne(booking => booking.Apartment)
            .WithMany(apartment => apartment.Bookings)
            .HasForeignKey(booking => booking.ApartmentId);

        modelBuilder.Entity<Booking>()
            .HasOne(booking => booking.Trip)
            .WithMany(trip => trip.Bookings)
            .HasForeignKey(booking => booking.TripId);

        modelBuilder.Entity<Destination>()
            .HasOne(destination => destination.Place)
            .WithMany(place => place.Destinations)
            .HasForeignKey(destination => destination.PlaceId);

        modelBuilder.Entity<Destination>()
            .HasOne(destination => destination.Trip)
            .WithMany(trip => trip.Destinations)
            .HasForeignKey(destination => destination.TripId);

        modelBuilder.Entity<Destination>()
            .HasMany(destination => destination.Tickets)
            .WithOne(ticket => ticket.Destination)
            .HasForeignKey(ticket => ticket.DestinationId);

        modelBuilder.Entity<Place>()
            .HasOne(place => place.City)
            .WithMany(city => city.Places)
            .HasForeignKey(place => place.CityId);

        modelBuilder.Entity<Place>()
            .HasMany(place => place.PlacePhotographs)
            .WithOne(photo => photo.Place)
            .HasForeignKey(photo => photo.PlaceId);

        modelBuilder.Entity<City>()
            .HasOne(city => city.Country)
            .WithMany(country => country.Cities)
            .HasForeignKey(city => city.CountryId);
    }
}
