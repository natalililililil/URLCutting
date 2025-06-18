using Microsoft.AspNetCore.Mvc;
using URLCutting.Data;
using Microsoft.EntityFrameworkCore;

namespace URLCutting.Controllers
{
    public class RedirectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedirectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("{code}")]
        public async Task<IActionResult> Index(string code)
        {
            var link = await _context.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (link == null)
                return NotFound();

            link.Clicks++;
            await _context.SaveChangesAsync();

            return Redirect(link.OriginalUrl);
        }
    }
}
