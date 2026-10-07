using System.Security.Claims;
using AcxiomCrm.Data;
using AcxiomCrm.Models;
using AcxiomCrm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCrm.Controllers;

[Authorize]
public sealed class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public FollowUpsController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index()
    {
        var query = _db.FollowUps.AsNoTracking();
        if (!TeamUser()) query = query.Where(x => x.AssignedTo == UserId());
        return View(await query.OrderBy(x => x.FollowUpDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View(new FollowUp { Status = "Planned", FollowUpDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        if (followUp.FollowUpDate.Date < DateTime.Today)
            ModelState.AddModelError(nameof(followUp.FollowUpDate), "Follow-up date cannot be earlier than today.");

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(followUp);
        }

        followUp.AssignedTo = UserId();
        _db.FollowUps.Add(followUp);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Create", "FollowUp", followUp.FollowUpId.ToString(), after: followUp);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var item = await _db.FollowUps.FindAsync(id);
        if (item is null) return NotFound();
        if (!TeamUser() && item.AssignedTo != UserId()) return Forbid();

        item.Status = "Completed";
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("Complete", "FollowUp", id.ToString(), after: new { item.Status });
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookups()
    {
        ViewData["Customers"] = await _db.Customers.AsNoTracking().OrderBy(x => x.CustomerName).ToListAsync();
        ViewData["Leads"] = await _db.Leads.AsNoTracking().OrderBy(x => x.LeadName).ToListAsync();
    }

    private bool TeamUser() => User.IsInRole("Admin") || User.IsInRole("Manager");
    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
}
