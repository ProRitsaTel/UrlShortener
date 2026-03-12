using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _db.ShortUrls
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                TempData["Error"] = "Введите ссылку.";
                return RedirectToAction(nameof(Index));
            }

            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                TempData["Error"] = "Введите корректный URL.";
                return RedirectToAction(nameof(Index));
            }

            var entity = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = await GenerateUniqueCodeAsync(),
                CreatedAtUtc = DateTime.UtcNow,
                ClickCount = 0
            };

            _db.ShortUrls.Add(entity);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.ShortUrls.FindAsync(id);
            if (entity == null)
                return NotFound();

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string originalUrl)
        {
            var entity = await _db.ShortUrls.FindAsync(id);
            if (entity == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                ModelState.AddModelError("", "Введите ссылку.");
                return View(entity);
            }

            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("", "Введите корректный URL.");
                return View(entity);
            }

            entity.OriginalUrl = originalUrl;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ShortUrls.FindAsync(id);
            if (entity != null)
            {
                _db.ShortUrls.Remove(entity);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> Go(string code)
        {
            var entity = await _db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (entity == null)
                return NotFound("Короткая ссылка не найдена.");

            entity.ClickCount++;
            await _db.SaveChangesAsync();

            return Redirect(entity.OriginalUrl);
        }

        private async Task<string> GenerateUniqueCodeAsync(int length = 6)
        {
            string code;

            do
            {
                code = GenerateCode(length);
            }
            while (await _db.ShortUrls.AnyAsync(x => x.ShortCode == code));

            return code;
        }

        private static string GenerateCode(int length)
        {

            return Guid.NewGuid().ToString().Substring(0, length);

        }
    }
}