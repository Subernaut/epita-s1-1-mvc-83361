using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Data;
using Library.MVC.ViewModels;

namespace Library.MVC.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Product)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }

        // GET: Invoices/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateInvoiceViewModel
            {
                InvoiceDate = DateTime.Today,
                Customers = await _context.Customers
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync(),

                Products = await _context.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Name} ({p.UnitPrice:C})"
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvoiceViewModel vm)
        {
            // Repopulate dropdowns if validation fails
            async Task LoadListsAsync()
            {
                vm.Customers = await _context.Customers
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                vm.Products = await _context.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Name} ({p.UnitPrice:C})"
                    })
                    .ToListAsync();
            }

            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return View(vm);
            }

            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == vm.CustomerId);

            if (!customerExists)
            {
                ModelState.AddModelError(nameof(vm.CustomerId), "Selected customer does not exist.");
                await LoadListsAsync();
                return View(vm);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == vm.ProductId);

            if (product == null)
            {
                ModelState.AddModelError(nameof(vm.ProductId), "Selected product does not exist.");
                await LoadListsAsync();
                return View(vm);
            }

            var invoice = new Invoice
            {
                InvoiceDate = vm.InvoiceDate,
                CustomerId = vm.CustomerId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        ProductId = vm.ProductId,
                        Quantity = vm.Quantity,
                        UnitPrice = product.UnitPrice // snapshot price
                    }
                }
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}
