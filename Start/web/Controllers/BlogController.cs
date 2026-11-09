using AtlasBlog.Data;
using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AtlasBlog.Controllers;

[Authorize(Policy = "AuthorRequired")]
public class BlogController : Controller
{
    private readonly ApplicationDbContext _context;

    public BlogController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> MyBlog()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var blog = await _context.Blogs
            .Include(b => b.Posts)
            .FirstOrDefaultAsync(b => b.AuthorId == userId);

        if (blog == null)
        {
            return NotFound();
        }

        return View(blog);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
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

        var model = new BlogEditViewModel
        {
            Title = blog.Title,
            Subtitle = blog.Subtitle,
            Description = blog.Description
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogEditViewModel model)
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

        blog.Title = model.Title;
        blog.Subtitle = model.Subtitle;
        blog.Description = model.Description;
        blog.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Blog updated successfully.";

        return RedirectToAction(nameof(MyBlog));
    }
}   

 