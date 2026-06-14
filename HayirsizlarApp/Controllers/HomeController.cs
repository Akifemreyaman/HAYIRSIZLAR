using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HayirsizlarApp.Data;
using HayirsizlarApp.Models;
using HayirsizlarApp.Services;

namespace HayirsizlarApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(AppDbContext context, IWebHostEnvironment environment, IEmailService emailService, IServiceProvider serviceProvider)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Tweets
                .Include(t => t.User)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .Where(t => t.ParentTweetId == null)
                .OrderByDescending(t => t.CreatedAt);

            int pageSize = 30;
            int totalTweets = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalTweets / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var tweets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allUsers = await _context.Users.ToListAsync();
            var totalUsers = allUsers.Count;

            var viewModel = new FeedViewModel
            {
                Tweets = tweets,
                CurrentUser = currentUser,
                TotalUserCount = totalUsers,
                NewTweet = new TweetPostViewModel(),
                CurrentPage = page,
                TotalPages = totalPages,
                AllUsers = allUsers
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostTweet([Bind(Prefix = "NewTweet")] TweetPostViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Custom validation: Either Content or MediaFile must be provided
            if (string.IsNullOrWhiteSpace(model.Content) && (model.MediaFile == null || model.MediaFile.Length == 0))
            {
                ModelState.AddModelError("NewTweet.Content", "Paylaşım yapabilmek için metin yazmalı veya bir medya (resim/video) eklemelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                // If model state is invalid, reload the feed with validation errors
                var currentUser = await _context.Users.FindAsync(userId);
                var query = _context.Tweets
                    .Include(t => t.User)
                    .Include(t => t.Replies)
                        .ThenInclude(r => r.User)
                    .Where(t => t.ParentTweetId == null)
                    .OrderByDescending(t => t.CreatedAt);

                int pageSize = 30;
                int totalTweets = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalTweets / pageSize);
                if (totalPages < 1) totalPages = 1;

                var tweets = await query
                    .Take(pageSize)
                    .ToListAsync();
                var allUsers = await _context.Users.ToListAsync();
                var totalUsers = allUsers.Count;

                var viewModel = new FeedViewModel
                {
                    Tweets = tweets,
                    CurrentUser = currentUser!,
                    TotalUserCount = totalUsers,
                    NewTweet = model,
                    CurrentPage = 1,
                    TotalPages = totalPages,
                    AllUsers = allUsers
                };
                return View("Index", viewModel);
            }

            var tweet = new Tweet
            {
                UserId = userId,
                Content = model.Content?.Trim(),
                CreatedAt = DateTime.UtcNow,
                MediaType = MediaType.None,
                ParentTweetId = model.ParentTweetId
            };

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                var extension = Path.GetExtension(model.MediaFile.FileName).ToLower();
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allowedVideoExtensions = new[] { ".mp4", ".webm", ".ogg" };

                bool isImage = allowedImageExtensions.Contains(extension);
                bool isVideo = allowedVideoExtensions.Contains(extension);

                if (!isImage && !isVideo)
                {
                    ModelState.AddModelError("NewTweet.MediaFile", "Sadece resim (jpg, png, webp, gif) veya video (mp4, webm) dosyaları yükleyebilirsiniz.");
                    
                    var currentUser = await _context.Users.FindAsync(userId);
                    var query = _context.Tweets
                        .Include(t => t.User)
                        .Include(t => t.Replies)
                            .ThenInclude(r => r.User)
                        .Where(t => t.ParentTweetId == null)
                        .OrderByDescending(t => t.CreatedAt);

                    int pageSize = 30;
                    int totalTweets = await query.CountAsync();
                    int totalPages = (int)Math.Ceiling((double)totalTweets / pageSize);
                    if (totalPages < 1) totalPages = 1;

                    var tweets = await query
                        .Take(pageSize)
                        .ToListAsync();
                    var allUsers = await _context.Users.ToListAsync();
                    var totalUsers = allUsers.Count;

                    var viewModel = new FeedViewModel
                    {
                        Tweets = tweets,
                        CurrentUser = currentUser!,
                        TotalUserCount = totalUsers,
                        NewTweet = model,
                        CurrentPage = 1,
                        TotalPages = totalPages,
                        AllUsers = allUsers
                    };
                    return View("Index", viewModel);
                }

                // Ensure uploads directory exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.MediaFile.CopyToAsync(fileStream);
                }

                tweet.MediaUrl = "/uploads/" + uniqueFileName;
                tweet.MediaType = isImage ? MediaType.Image : MediaType.Video;
            }

            _context.Tweets.Add(tweet);
            await _context.SaveChangesAsync();

            // Mention detection and Notification creation
            if (!string.IsNullOrEmpty(tweet.Content))
            {
                var matches = Regex.Matches(tweet.Content, @"\B@([a-zA-Z0-9_]{3,20})\b");
                var mentionedUsernames = matches.Select(m => m.Groups[1].Value.ToLower()).Distinct().ToList();

                var sender = await _context.Users.FindAsync(userId);
                var senderName = sender?.DisplayName ?? "Bir yazar";

                foreach (var username in mentionedUsernames)
                {
                    var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (receiver != null && receiver.Id != userId)
                    {
                        var notification = new Notification
                        {
                            ReceiverUserId = receiver.Id,
                            SenderUserId = userId,
                            Message = tweet.ParentTweetId.HasValue 
                                ? $"{senderName} bir cevapta sizden bahsetti." 
                                : $"{senderName} bir günlükte sizden bahsetti.",
                            TweetId = tweet.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                    }
                }
            }

            // If it's a reply, notify the owner of the parent tweet (unless it's the owner themselves replying)
            if (tweet.ParentTweetId.HasValue)
            {
                var parentTweet = await _context.Tweets.FindAsync(tweet.ParentTweetId.Value);
                if (parentTweet != null && parentTweet.UserId != userId)
                {
                    var sender = await _context.Users.FindAsync(userId);
                    var senderName = sender?.DisplayName ?? "Bir yazar";

                    // Check if notification already exists to avoid duplicate
                    var exists = await _context.Notifications.AnyAsync(n => n.TweetId == tweet.Id && n.ReceiverUserId == parentTweet.UserId);
                    if (!exists)
                    {
                        var notification = new Notification
                        {
                            ReceiverUserId = parentTweet.UserId,
                            SenderUserId = userId,
                            Message = $"{senderName} günlüğünüze bir cevap yazdı.",
                            TweetId = tweet.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Trigger background email sending to other users (with 6 hours rate limit)
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var author = await dbContext.Users.FindAsync(userId);
                        var authorName = author?.DisplayName ?? "Hayırsızlar Yazar";

                        var contentSnippet = !string.IsNullOrEmpty(model.Content)
                            ? (model.Content.Length > 150 ? model.Content.Substring(0, 150) + "..." : model.Content)
                            : (tweet.MediaType == MediaType.Image ? "[Bir Görsel Paylaşıldı]" : "[Bir Video Paylaşıldı]");

                        var sixHoursAgo = DateTime.UtcNow.AddHours(-6);
                        var targetUsers = await dbContext.Users
                            .Where(u => u.Id != userId && u.Email != null && u.Email != "" && 
                                       (u.LastEmailSentAt == null || u.LastEmailSentAt < sixHoursAgo))
                            .ToListAsync();

                        if (targetUsers.Any())
                        {
                            var subject = "Hayırsızlar'da Yeni Güncellemeler Var!";
                            var isComment = model.ParentTweetId.HasValue;
                            var postType = isComment ? "yeni bir yorum" : "yeni bir günlük";
                            
                            foreach (var user in targetUsers)
                            {
                                var body = $@"
                                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 3px solid #121212; background-color: #F4F1EA; max-width: 600px;'>
                                        <h2 style='text-transform: uppercase; border-bottom: 3px solid #121212; padding-bottom: 10px;'>Hayırsızlar Log Güncellemesi</h2>
                                        <p>Merhaba <strong>{user.DisplayName}</strong>,</p>
                                        <p>Akışta {postType} paylaşıldı!</p>
                                        <div style='background-color: #ffffff; padding: 15px; border: 2px solid #121212; margin: 15px 0;'>
                                            <p style='margin: 0; font-size: 0.85rem; color: #555; text-transform: uppercase;'>Gönderen: @{author?.Username} ({authorName})</p>
                                            <hr style='border: 1px dashed #121212;' />
                                            <p style='margin: 5px 0 0 0; font-style: italic;'>""{contentSnippet}""</p>
                                        </div>
                                        <p style='margin-top: 20px;'><a href='http://hayirsizlar.com' style='display: inline-block; padding: 10px 20px; background-color: #FFE600; color: #121212; text-decoration: none; font-weight: bold; border: 2px solid #121212;'>SİTEYİ ZİYARET ET &rarr;</a></p>
                                    </div>";

                                await emailSender.SendEmailAsync(user.Email!, subject, body);
                                user.LastEmailSentAt = DateTime.UtcNow;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Background email error: " + ex.Message);
                }
            });

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTweet(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var tweet = await _context.Tweets
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (tweet == null)
            {
                return NotFound();
            }

            // Users can only delete their own tweets
            if (tweet.UserId != userId)
            {
                return Forbid();
            }

            // Collect all tweets to delete (parent + comments)
            var allTweetsToDelete = new List<Tweet> { tweet };
            if (tweet.Replies != null && tweet.Replies.Any())
            {
                allTweetsToDelete.AddRange(tweet.Replies);
            }

            var tweetIdsToDelete = allTweetsToDelete.Select(t => t.Id).ToList();

            // Remove files from storage
            foreach (var t in allTweetsToDelete)
            {
                if (!string.IsNullOrEmpty(t.MediaUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, t.MediaUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch
                        {
                            // Ignore log file deletion issues
                        }
                    }
                }
            }

            // Delete notifications pointing to these tweets
            var relatedNotifications = await _context.Notifications
                .Where(n => n.TweetId.HasValue && tweetIdsToDelete.Contains(n.TweetId.Value))
                .ToListAsync();

            if (relatedNotifications.Any())
            {
                _context.Notifications.RemoveRange(relatedNotifications);
            }

            // Delete replies first, then parent tweet
            if (tweet.Replies != null && tweet.Replies.Any())
            {
                _context.Tweets.RemoveRange(tweet.Replies);
            }
            _context.Tweets.Remove(tweet);
            
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _context.Notifications
                .Include(n => n.SenderUser)
                .Include(n => n.Tweet)
                .Where(n => n.ReceiverUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
            if (unreadNotifications.Any())
            {
                unreadNotifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> Tweet(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tweet = await _context.Tweets
                .Include(t => t.User)
                .Include(t => t.ParentTweet)
                    .ThenInclude(p => p!.User)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tweet == null)
            {
                return NotFound();
            }

            // If this is a reply itself, redirect to the parent tweet detail page and highlight this reply
            if (tweet.ParentTweetId.HasValue)
            {
                return RedirectToAction("Tweet", "Home", new { id = tweet.ParentTweetId.Value }, $"reply-{tweet.Id}");
            }

            var allUsers = await _context.Users.ToListAsync();

            var viewModel = new TweetDetailViewModel
            {
                Tweet = tweet,
                CurrentUser = currentUser,
                AllUsers = allUsers,
                NewReply = new TweetPostViewModel { ParentTweetId = tweet.Id }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTweet(int id, string content)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var tweet = await _context.Tweets.FindAsync(id);
            if (tweet == null)
            {
                return NotFound();
            }

            if (tweet.UserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(content) && string.IsNullOrEmpty(tweet.MediaUrl))
            {
                TempData["ErrorMessage"] = "Günlük veya yorum içeriği boş olamaz.";
                var refererUrl = Request.Headers["Referer"].ToString();
                return !string.IsNullOrEmpty(refererUrl) ? Redirect(refererUrl) : RedirectToAction("Index");
            }

            tweet.Content = content?.Trim();
            tweet.IsEdited = true;
            tweet.EditedAt = DateTime.UtcNow;

            // Mention detection and Notification creation for new mentions in edited content
            if (!string.IsNullOrEmpty(tweet.Content))
            {
                var matches = Regex.Matches(tweet.Content, @"\B@([a-zA-Z0-9_]{3,20})\b");
                var mentionedUsernames = matches.Select(m => m.Groups[1].Value.ToLower()).Distinct().ToList();

                var sender = await _context.Users.FindAsync(userId);
                var senderName = sender?.DisplayName ?? "Bir yazar";

                foreach (var username in mentionedUsernames)
                {
                    var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (receiver != null && receiver.Id != userId)
                    {
                        var exists = await _context.Notifications.AnyAsync(n => n.TweetId == tweet.Id && n.ReceiverUserId == receiver.Id);
                        if (!exists)
                        {
                            var notification = new Notification
                            {
                                ReceiverUserId = receiver.Id,
                                SenderUserId = userId,
                                Message = tweet.ParentTweetId.HasValue 
                                    ? $"{senderName} bir cevapta sizden bahsetti." 
                                    : $"{senderName} bir günlükte sizden bahsetti.",
                                TweetId = tweet.Id,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }
    }
}
