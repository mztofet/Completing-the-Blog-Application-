using AtlasBlog.Data;
using AtlasBlog.Models.EF;
using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AtlasBlog.Controllers;

[Authorize(Policy = "AuthorRequired")]
public class PostController : Controller
{
    private readonly ApplicationDbContext _context;

    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PostEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var blog = await _context.Blogs
            .FirstOrDefaultAsync(b => b.AuthorId == userId);

        if (blog == null)
        {
            return NotFound();
        }

        var post = new Post
        {
            Title = model.Title,
            Content = model.Content,
            BlogId = blog.BlogId,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PostEditViewModel model)
    {
        return View(model);
    }
}