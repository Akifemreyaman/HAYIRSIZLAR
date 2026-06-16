using System;

namespace HayirsizlarApp.Models
{
    public class TweetLikeDislike
    {
        public int Id { get; set; }
        
        public int TweetId { get; set; }
        public Tweet? Tweet { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public bool IsLike { get; set; } // true if Like, false if Dislike
    }
}
