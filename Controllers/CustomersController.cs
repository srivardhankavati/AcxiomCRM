using System.Security.Claims;
using AcxiomCrm.Data;
using AcxiomCrm.Models;
using AcxiomCrm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCrm.Controllers;

[Authorize]
public sealed class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public CustomersController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = VisibleCustomers();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.CustomerName.Contains(term) ||
                                     c.Email.Contains(term) ||
                                     c.Phone.Contains(term) ||
                                     (c.CompanyName ?? "").Contains(term));
        }

        ViewData["Search"] = search;
        var customers = await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await VisibleCustomers().FirstOrDefaultAsync(c => c.CustomerId == id);
        return customer is null ? NotFound() : View(customer);
    }

    public IActionResult Create() => View(new Customer { Status = "Active" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        await AddDuplicateErrors(customer);
        if (!ModelState.IsValid) return View(customer);

        customer.CustomerCode = MakeCode("CUS");
        customer.CreatedBy = CurrentUserId();
        customer.CreatedDate = DateTime.UtcNow;

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Create", "Customer", customer.CustomerId.ToString(), after: customer);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await VisibleCustomers().FirstOrDefaultAsync(c => c.CustomerId == id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer input)
    {
        if (id != input.CustomerId) return BadRequest();

        var existing = await VisibleCustomers().FirstOrDefaultAsync(c => c.CustomerId == id);
        if (existing is null) return NotFound();

        await AddDuplicateErrors(input, id);
        if (!ModelState.IsValid) return View(input);

        var before = new
        {
            existing.CustomerName,
            existing.Email,
            existing.Phone,
            existing.CompanyName,
            existing.Address,
            existing.City,
            existing.State,
            existing.Status
        };

        existing.CustomerName = input.CustomerName;
        existing.Email = input.Email;
        existing.Phone = input.Phone;
        existing.CompanyName = input.CompanyName;
        existing.Address = input.Address;
        existing.City = input.City;
        existing.State = input.State;
        existing.Status = input.Status;
        existing.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Update", "Customer", id.ToString(), before, existing);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Delete", "Customer", id.ToString(), before: customer);
        return RedirectToAction(nameof(Index));
    }

    private IQueryable<Customer> VisibleCustomers()
    {
        var query = _db.Customers.AsNoTracking();
        if (User.IsInRole("Admin") || User.IsInRole("Manager")) return query;

        var userId = CurrentUserId();
        return query.Where(c => c.CreatedBy == userId);
    }

    private async Task AddDuplicateErrors(Customer customer, int? ignoreId = null)
    {
        var duplicate = await _db.Customers.AnyAsync(c =>
            (!ignoreId.HasValue || c.CustomerId != ignoreId.Value) &&
            (c.Email == customer.Email || c.Phone == customer.Phone));

        if (duplicate)
            ModelState.AddModelError("", "A customer with the supplied email or phone already exists.");
    }

    private string CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private static string MakeCode(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
}
