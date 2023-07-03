﻿namespace TripFrog.Models;

public class LanguageUser
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid LanguageId { get; set; }
    public Language Language { get; set; }
}
