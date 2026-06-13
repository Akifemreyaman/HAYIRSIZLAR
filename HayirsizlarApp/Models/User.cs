using System;
using System.Collections.Generic;

namespace HayirsizlarApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#FFE600"; // Default yellow
        public string? MoodStatus { get; set; }
        public string? Email { get; set; }
        public DateTime? LastEmailSentAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();
    }
}
