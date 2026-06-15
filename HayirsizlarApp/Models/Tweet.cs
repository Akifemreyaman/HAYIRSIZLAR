using System;

namespace HayirsizlarApp.Models
{
    public enum MediaType
    {
        None,
        Image,
        Video
    }

    public class Tweet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string? Content { get; set; }
        public int? ParentTweetId { get; set; }
        public Tweet? ParentTweet { get; set; }
        public ICollection<Tweet> Replies { get; set; } = new List<Tweet>();
        public MediaType MediaType { get; set; } = MediaType.None;
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public int? QuoteTweetId { get; set; }
        public Tweet? QuoteTweet { get; set; }
    }
}
