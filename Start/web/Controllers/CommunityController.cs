using AtlasBlog.Data;
using AtlasBlog.Models.EF;
using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .Include(p => p.Blog)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
        {
            return NotFound();
        }

        bool canSeeHidden =
            User.IsInRole("Admin") ||
            User.IsInRole("Moderator");

        var model = new PostDetailViewModel
        {
            PostId = post.PostId,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            LastModifiedAt = post.LastModifiedAt,
            BlogId = post.BlogId,
            BlogTitle = post.Blog.Title,

            Comments = post.Comments
                .Where(c => c.IsVisible || canSeeHidden)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentViewModel
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserEmail = c.User.Email ?? string.Empty,
                    IsVisible = c.IsVisible
                })
                .ToList()
        };

        return View(model);
    }

    // POST: Community/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(CreateCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(ViewPost),
                new { id = model.PostId });
        }

        var comment = new Comment
        {
            Content = model.Content,
            PostId = model.PostId,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            CreatedAt = DateTime.UtcNow,
            IsVisible = true
        };

        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ViewPost),
            new { id = model.PostId });
    }

    // POST: Community/ToggleCommentVisibility
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ModeratorOrAdmin")]
    public async Task<IActionResult> ToggleCommentVisibility(int commentId, int postId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        comment.IsVisible = !comment.IsVisible;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ViewPost),
            new { id = postId });
    }

    // GET: Community/ViewAllPosts/5
    public async Task<IActionResult> ViewAllPosts(int? blogId)
    {
        if (blogId == null)
        {
            return NotFound();
        }

            var blog = await _context.Blogs
            .Include(b => b.Posts)
            .FirstOrDefaultAsync(b => b.BlogId == blogId);

        if (blog == null)
        {
            return NotFound();
        }

        var model = new BlogPostsViewModel
        {
            BlogId = blog.BlogId,
            BlogTitle = blog.Title,
            BlogSubtitle = blog.Subtitle,

            Posts = blog.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostSummaryViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    CreatedAt = p.CreatedAt
                })
                .ToList()
        };

        return View(model);
    }
    }