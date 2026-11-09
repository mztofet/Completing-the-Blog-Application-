using AtlasBlog.Data;
using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AtlasBlog.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public HomeController(
        ApplicationDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        const string cacheKey = "BlogList";

        if (!_cache.TryGetValue(
            cacheKey,
            out List<BlogListViewModel>? blogs))
        {
            blogs = await _context.Blogs
                .Include(b => b.Author)
                .Select(b => new BlogListViewModel
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Subtitle = b.Subtitle,
                    AuthorEmail = b.Author == null
                        ? string.Empty
                        : b.Author.Email ?? string.Empty
                })
                .ToListAsync();

            _cache.Set(
                cacheKey,
                blogs,
                TimeSpan.FromMinutes(30));
        }

        return View(blogs);
    }
}