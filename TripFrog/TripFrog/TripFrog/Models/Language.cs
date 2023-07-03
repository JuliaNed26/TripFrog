namespace TripFrog.Models;

public class Language
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IList<LanguageUser> LanguageUsers { get; set; }
}
