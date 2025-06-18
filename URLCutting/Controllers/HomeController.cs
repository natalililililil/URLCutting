using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLCutting.Data;
using URLCutting.Models;

namespace URLCutting.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var links = _context.ShortUrls.OrderByDescending(x => x.CreatedAt).ToList();
            return View(links);
        }

        [HttpPost]
        public IActionResult Create(string originalUrl)
        {
            string errorCode = null;

            if (string.IsNullOrWhiteSpace(originalUrl))
                errorCode = "empty";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(originalUrl, @"^[a-zA-Z0-9\-._~]+$"))
                errorCode = "invalid";
            else if (originalUrl.Length > 2048)
                errorCode = "tooLong";
            else if (_context.ShortUrls.Any(x => x.OriginalUrl == originalUrl))
                errorCode = "exists";

            switch (errorCode)
            {
                case "empty":
                    ViewBag.Message = "Введите корректную ссылку";
                    break;
                case "invalid":
                    ViewBag.Message = "Ссылка не должна содержать недопустимые символы";
                    break;
                case "tooLong":
                    ViewBag.Message = "Ссылка слишком длинная";
                    break;
                case "exists":
                    ViewBag.Message = "Такая ссылка уже существует";
                    break;
                case null:
                    var shortUrl = new ShortUrl
                    {
                        OriginalUrl = originalUrl,
                        CreatedAt = DateTime.UtcNow,
                        Clicks = 0,
                        ShortCode = null
                    };

                    _context.ShortUrls.Add(shortUrl);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
            }

            var links = _context.ShortUrls.OrderByDescending(x => x.CreatedAt).ToList();
            return View("Index", links);
        }


        [HttpPost]
        public IActionResult GenerateCode(int id)
        {
            var link = _context.ShortUrls.FirstOrDefault(x => x.Id == id);
            if (link == null)
                return NotFound();

            if (string.IsNullOrEmpty(link.ShortCode))
            {
                link.ShortCode = GenerateShortCode();
                _context.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = link.Id });
        }


        private string GenerateShortCode() => Guid.NewGuid().ToString("N").Substring(0, 6);

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var link = await _context.ShortUrls.FindAsync(id);
            if (link == null)
                return NotFound();

            return View(link);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ShortUrl updatedLink)
        {
            var link = await _context.ShortUrls.FindAsync(id);
            if (link == null)
                return NotFound();

            if (!string.IsNullOrEmpty(updatedLink.ShortCode) &&
                !string.IsNullOrEmpty(updatedLink.OriginalUrl) &&
                updatedLink.ShortCode.Length >= updatedLink.OriginalUrl.Length)
            {
                ModelState.AddModelError("ShortCode", "Сокращенный URL не может быть длиннее или равен оригинальному URL");
                return View(updatedLink);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(updatedLink.ShortCode, @"^[a-zA-Z0-9\-._~]+$"))
            {
                ModelState.AddModelError("ShortCode", "Ссылка не должна содержать недопустимые символы");
                return View(updatedLink);
            }



            link.OriginalUrl = updatedLink.OriginalUrl;
            link.ShortCode = updatedLink.ShortCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Не удалось сохранить изменения. Возможно, такой сокращенный код уже существует.");
                return View(updatedLink);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var link = await _context.ShortUrls.FindAsync(id);
            if (link == null)
                return NotFound();

            _context.ShortUrls.Remove(link);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Details(string code)
        {
            if (string.IsNullOrEmpty(code)) return NotFound();

            var link = _context.ShortUrls.FirstOrDefault(x => x.ShortCode == code);
            if (link == null) return NotFound();


            //link.Clicks++;
            //_context.SaveChanges();
            return View(link);
        }
    }
}