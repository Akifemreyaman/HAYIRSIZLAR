using System;

namespace HayirsizlarApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        public int ReceiverUserId { get; set; }
        public User? ReceiverUser { get; set; }
        
        public int SenderUserId { get; set; }
        public User? SenderUser { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public int? TweetId { get; set; }
        public Tweet? Tweet { get; set; }
        
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
