using AtlasBlog.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBlog.Controllers;

public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(new List<BlogListViewModel>
        {
            new BlogListViewModel
            {
                BlogId = 1,
                Title = "Alicia McDaniel",
                Subtitle = "Web Development Projects and Learning Journey",
                AuthorEmail = "mztofet@gmail.com"
            }
        });
    }
}