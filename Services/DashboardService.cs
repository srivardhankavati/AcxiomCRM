using AcxiomCrm.Data;
using AcxiomCrm.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCrm.Services;

public sealed class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardViewModel> BuildAsync(string? userId, bool canSeeTeamData)
    {
        var customers = _context.Customers.AsNoTracking().AsQueryable();
        var leads = _context.Leads.AsNoTracking().AsQueryable();
        var opportunities = _context.Opportunities.AsNoTracking().AsQueryable();

        if (!canSeeTeamData && !string.IsNullOrWhiteSpace(userId))
        {
            customers = customers.Where(x => x.CreatedBy == userId);
            leads = leads.Where(x => x.AssignedTo == userId);
            opportunities = opportunities.Where(x => x.AssignedTo == userId);
        }

        var model = new DashboardViewModel
        {
            TotalCustomers = await customers.CountAsync(),
            TotalLeads = await leads.CountAsync(),
            OpenLeads = await leads.CountAsync(x => x.Status != "Lost" && x.Status != "Converted"),
            TotalOpportunities = await opportunities.CountAsync(),
            OpenOpportunities = await opportunities.CountAsync(x => x.Status == "Open"),
            WonOpportunities = await opportunities.CountAsync(x => x.Status == "Won"),
            LostOpportunities = await opportunities.CountAsync(x => x.Status == "Lost"),

            Pipeline = (await opportunities
                .Where(x => x.Status == "Open")
                .Select(x => x.Amount)
                .ToListAsync())
                .Sum()
        };

        model.LeadStatus = await leads
            .GroupBy(x => x.Status)
            .Select(group => new ChartItem { Label = group.Key, Count = group.Count() })
            .OrderBy(x => x.Label)
            .ToListAsync();

        model.OpportunityStage = await opportunities
            .GroupBy(x => x.Stage)
            .Select(group => new ChartItem { Label = group.Key, Count = group.Count() })
            .OrderBy(x => x.Label)
            .ToListAsync();

        return model;
    }

    public Task<DashboardViewModel> GetAsync(string? userId, bool isAdmin, bool isManager)
        => BuildAsync(userId, isAdmin || isManager);
}