﻿namespace TripFrogModels.Models;

public sealed class ApartmentPhoto
{
    public Guid Id { get; set; }
    public string PhotoUrl { get; set; }
    public Guid ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }
}
