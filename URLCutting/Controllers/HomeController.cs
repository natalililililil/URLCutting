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
        public async Task<IActionResult> Create(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
            {
                ModelState.AddModelError("", "Введите ссылку.");
                return View("Index");
            }

            var existing = await _context.ShortUrls.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);

            if (existing != null)
            {
                ViewBag.ShortLink = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}";
                ViewBag.Message = "Такая ссылка уже была добавлена ранее";
                return View("Index");
            }

            var code = GenerateShortCode();

            var shortUrl = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = code
            };

            _context.ShortUrls.Add(shortUrl);
            await _context.SaveChangesAsync();

            ViewBag.ShortLink = $"{Request.Scheme}://{Request.Host}/{code}";
            return View("Index");
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

            link.OriginalUrl = updatedLink.OriginalUrl;
            link.ShortCode = updatedLink.ShortCode;

            await _context.SaveChangesAsync();

            return Redirect($"/{link.ShortCode}");
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
    }
}
