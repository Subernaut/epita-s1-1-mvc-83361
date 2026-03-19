using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Data;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString, string categoryFilter, string availabilityFilter)
    {
        IQueryable<Book> booksQuery = _context.Books;

        // Search
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            booksQuery = booksQuery.Where(b =>
                b.Title.Contains(searchString) || b.Author.Contains(searchString));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
        {
            booksQuery = booksQuery.Where(b => b.Category == categoryFilter);
        }

        // Availability filter
        if (!string.IsNullOrWhiteSpace(availabilityFilter) && availabilityFilter != "All")
        {
            if (availabilityFilter == "Available")
                booksQuery = booksQuery.Where(b => b.IsAvailable);
            else if (availabilityFilter == "OnLoan")
                booksQuery = booksQuery.Where(b => !b.IsAvailable);
        }

        // Sort by Title
        booksQuery = booksQuery.OrderBy(b => b.Title);

        // Build dropdowns in controller
        var categories = await _context.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.CategoryList = categories
            .Select(c => new SelectListItem
            {
                Text = c,
                Value = c,
                Selected = c == categoryFilter
            }).ToList();

        ViewBag.AvailabilityList = new List<SelectListItem>
        {
            new SelectListItem { Text = "All", Value = "All", Selected = availabilityFilter=="All" || string.IsNullOrEmpty(availabilityFilter) },
            new SelectListItem { Text = "Available", Value = "Available", Selected = availabilityFilter=="Available" },
            new SelectListItem { Text = "On Loan", Value = "OnLoan", Selected = availabilityFilter=="OnLoan" },
        };

        ViewBag.CurrentSearch = searchString;

        return View(await booksQuery.ToListAsync());
    }
}