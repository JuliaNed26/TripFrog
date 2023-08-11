using System.ComponentModel.DataAnnotations;

namespace TripFrogModels.Models;
public sealed class RefreshToken
{
    [Key]
    public Guid Token { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}
