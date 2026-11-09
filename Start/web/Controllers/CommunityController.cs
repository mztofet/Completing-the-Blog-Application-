using AtlasBlog.Data;
using AtlasBlog.Models.EF;
using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasBlog.Controllers;

public class CommunityController : Controller
{   

    private readonly ApplicationDbContext _context;

    public CommunityController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Community/ViewBlog/5
    public async Task<IActionResult> ViewBlog(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Posts)
            .FirstOrDefaultAsync(b => b.BlogId == id);

        if (blog == null)
        {
            return NotFound();
        }

        var model = new BlogDetailViewModel
        {
            BlogId = blog.BlogId,
            Title = blog.Title,
            Subtitle = blog.Subtitle,
            Description = blog.Description,
            AuthorEmail = blog.Author.Email ?? string.Empty,

            RecentPosts = blog.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => (dynamic)new PostSummaryViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    CreatedAt = p.CreatedAt
                })
                .ToList()
        };

        return View(model);
    }

    // GET: Community/ViewPost/5
    public async Task<IActionResult> ViewPost(int? id)
    {
        return View();
    }

    // POST: Community/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(CreateCommentViewModel model)
    {
        return RedirectToAction(nameof(ViewPost), new { id = model.PostId });
    }       
}
