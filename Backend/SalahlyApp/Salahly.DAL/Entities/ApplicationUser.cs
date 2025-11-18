using Microsoft.AspNetCore.Identity;
using Salahly.DAL.Entities;
using System.ComponentModel.DataAnnotations;

public enum UserType
{
    Admin,
    Customer,
    Craftsman
}

public class ApplicationUser : IdentityUser<int>
{
    [Required, MaxLength(100)]
    public string FullName { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public UserType UserType { get; set; }
    public bool IsProfileCompleted { get; set; } = false;

    public double RatingAverage { get; set; }

    // Navigation Properties
    public Admin? Admin { get; set; }
    public Customer? Customer { get; set; }
    public Craftsman? Craftsman { get; set; }

    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}
