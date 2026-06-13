using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HayirsizlarApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 20 karakter arasında olmalıdır.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görünen ad zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Görünen ad en az 2 karakter olmalıdır.")]
        [Display(Name = "Görünen Ad")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Davet kodu zorunludur.")]
        [Display(Name = "Davet Kodu")]
        public string RegistrationCode { get; set; } = string.Empty;

        [Display(Name = "Profil Rengi")]
        public string AvatarColor { get; set; } = "#FFE600";
    }

    public class TweetPostViewModel
    {
        public string? Content { get; set; }

        public IFormFile? MediaFile { get; set; }

        public int? ParentTweetId { get; set; }
    }

    public class ChangeProfileViewModel
    {
        [Required(ErrorMessage = "Görünen ad zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Görünen ad en az 2 karakter olmalıdır.")]
        [Display(Name = "Görünen Ad")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profil rengi zorunludur.")]
        [Display(Name = "Profil Rengi")]
        public string AvatarColor { get; set; } = "#FFE600";

        [StringLength(30, ErrorMessage = "Ruh hali durumu en fazla 30 karakter olmalıdır.")]
        [Display(Name = "Şu Anki Ruh Halin")]
        public string? MoodStatus { get; set; }

        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi.")]
        [Display(Name = "E-posta Adresi (İsteğe Bağlı)")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre (Şifre değiştirmek için)")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifreyi Onayla")]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string? ConfirmNewPassword { get; set; }
    }

    public class FeedViewModel
    {
        public List<Tweet> Tweets { get; set; } = new List<Tweet>();
        public TweetPostViewModel NewTweet { get; set; } = new TweetPostViewModel();
        public User CurrentUser { get; set; } = null!;
        public int TotalUserCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<User> AllUsers { get; set; } = new List<User>();
    }
}
