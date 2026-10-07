using System.Security.Claims;
using AcxiomCrm.Data;
using AcxiomCrm.Models;
using AcxiomCrm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCrm.Controllers;

[Authorize]
public sealed class LeadsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public LeadsController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Leads.AsNoTracking();
        if (!CanViewTeam()) query = query.Where(x => x.AssignedTo == CurrentUser());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.LeadName.Contains(term) ||
                                     (x.CompanyName ?? "").Contains(term) ||
                                     x.Status.Contains(term));
        }

        ViewData["Search"] = search;
        return View(await query.OrderByDescending(x => x.CreatedDate).ToListAsync());
    }

    public IActionResult Create() => View(new Lead { Status = "New", Priority = "Normal" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lead lead)
    {
        if (!IsLeadStatusValid(lead.Status))
            ModelState.AddModelError(nameof(lead.Status), "Choose a valid lead status.");

        if (!ModelState.IsValid) return View(lead);

        lead.LeadCode = $"LEAD-{Guid.NewGuid():N}"[..13].ToUpperInvariant();
        lead.AssignedTo = CurrentUser();
        lead.CreatedDate = DateTime.UtcNow;

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Create", "Lead", lead.LeadId.ToString(), after: lead);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        if (lead is null || (!CanViewTeam() && lead.AssignedTo != CurrentUser())) return NotFound();
        return View(lead);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lead input)
    {
        if (id != input.LeadId) return BadRequest();

        var lead = await _db.Leads.FindAsync(id);
        if (lead is null || (!CanViewTeam() && lead.AssignedTo != CurrentUser())) return NotFound();

        if (!IsLeadStatusValid(input.Status))
            ModelState.AddModelError(nameof(input.Status), "Choose a valid lead status.");

        if (!ModelState.IsValid) return View(input);

        var before = new { lead.LeadName, lead.Email, lead.Phone, lead.Status, lead.Priority, lead.ExpectedValue };
        lead.LeadName = input.LeadName;
        lead.Email = input.Email;
        lead.Phone = input.Phone;
        lead.CompanyName = input.CompanyName;
        lead.Source = input.Source;
        lead.Status = input.Status;
        lead.Priority = input.Priority;
        lead.ExpectedValue = input.ExpectedValue;
        lead.Notes = input.Notes;

        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Update", "Lead", id.ToString(), before, lead);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        return lead is null ? NotFound() : View(lead);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        if (lead is null) return NotFound();
        _db.Leads.Remove(lead);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Delete", "Lead", id.ToString(), before: lead);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Convert(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        if (lead is null) return NotFound();
        if (lead.Status != "Qualified")
        {
            TempData["Message"] = "Only qualified leads can be converted.";
            return RedirectToAction(nameof(Index));
        }

        if (await _db.Customers.AnyAsync(c => c.Email == lead.Email || c.Phone == lead.Phone))
        {
            TempData["Message"] = "A matching customer already exists.";
            return RedirectToAction(nameof(Index));
        }

        var customer = new Customer
        {
            CustomerCode = $"CUS-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            CustomerName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            Status = "Active",
            CreatedBy = CurrentUser(),
            CreatedDate = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        lead.Status = "Converted";
        await _db.SaveChangesAsync();

        var opportunity = new Opportunity
        {
            OpportunityName = $"{lead.LeadName} - Initial Opportunity",
            CustomerId = customer.CustomerId,
            LeadId = lead.LeadId,
            Amount = lead.ExpectedValue > 0 ? lead.ExpectedValue : 1,
            Stage = "Qualification",
            Probability = 20,
            ExpectedCloseDate = DateTime.Today.AddDays(30),
            Status = "Open",
            AssignedTo = lead.AssignedTo ?? CurrentUser()
        };

        _db.Opportunities.Add(opportunity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Convert", "Lead", id.ToString(), after: new { customer.CustomerId, opportunity.OpportunityId });

        TempData["Message"] = "Lead converted into a customer and opportunity.";
        return RedirectToAction(nameof(Index));
    }

    private bool CanViewTeam() => User.IsInRole("Admin") || User.IsInRole("Manager");
    private string CurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private static bool IsLeadStatusValid(string? status) =>
        new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" }
            .Contains(status, StringComparer.OrdinalIgnoreCase);
}
