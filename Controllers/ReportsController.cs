using AcxiomCrm.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCrm.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var isTeam = User.IsInRole("Admin") || User.IsInRole("Manager");
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var customers = _db.Customers.AsQueryable();
        var leads = _db.Leads.AsQueryable();
        var opps = _db.Opportunities.AsQueryable();
        var follow = _db.FollowUps.AsQueryable();

        if (!isTeam)
        {
            customers = customers.Where(x => x.CreatedBy == id);
            leads = leads.Where(x => x.AssignedTo == id);
            opps = opps.Where(x => x.AssignedTo == id);
            follow = follow.Where(x => x.AssignedTo == id);
        }

        // Get open opportunity amounts first.
        // SQLite cannot directly SUM decimal values.
        var openOpportunityAmounts = await opps
            .Where(x => x.Status == "Open")
            .Select(x => new
            {
                x.Amount,
                x.Probability
            })
            .ToListAsync();

        // Calculate decimal sums in C# instead of SQLite.
        var pipeline = openOpportunityAmounts
            .Sum(x => x.Amount);

        var weightedPipeline = openOpportunityAmounts
            .Sum(x => x.Amount * x.Probability / 100m);

        var vm = new ReportsViewModel
        {
            Customers = await customers
                .OrderByDescending(x => x.CreatedDate)
                .Take(100)
                .ToListAsync(),

            Leads = await leads
                .OrderByDescending(x => x.CreatedDate)
                .Take(100)
                .ToListAsync(),

            Opportunities = await opps
                .OrderByDescending(x => x.CreatedDate)
                .Take(100)
                .ToListAsync(),

            FollowUps = await follow
                .OrderBy(x => x.FollowUpDate)
                .Take(100)
                .ToListAsync(),

            Pipeline = pipeline,

            WeightedPipeline = weightedPipeline
        };

        return View(vm);
    }
}

public class ReportsViewModel
{
    public List<AcxiomCrm.Models.Customer> Customers { get; set; } = [];
    public List<AcxiomCrm.Models.Lead> Leads { get; set; } = [];
    public List<AcxiomCrm.Models.Opportunity> Opportunities { get; set; } = [];
    public List<AcxiomCrm.Models.FollowUp> FollowUps { get; set; } = [];

    public decimal Pipeline { get; set; }
    public decimal WeightedPipeline { get; set; }
}