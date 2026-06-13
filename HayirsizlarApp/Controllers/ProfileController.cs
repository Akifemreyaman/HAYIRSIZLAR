using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HayirsizlarApp.Data;
using HayirsizlarApp.Models;

namespace HayirsizlarApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ProfileController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new ChangeProfileViewModel
            {
                DisplayName = user.DisplayName,
                AvatarColor = user.AvatarColor,
                MoodStatus = user.MoodStatus,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ChangeProfileViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // If user attempts to change password
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Şifrenizi değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
                if (verification == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifreniz yanlış.");
                    return View(model);
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            }

            user.DisplayName = model.DisplayName.Trim();
            user.AvatarColor = model.AvatarColor;
            user.MoodStatus = model.MoodStatus?.Trim();
            user.Email = model.Email?.Trim();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Refresh authentication cookie claims to update navbar and user UI without requiring logout/login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("DisplayName", user.DisplayName),
                new Claim("AvatarColor", user.AvatarColor)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}
