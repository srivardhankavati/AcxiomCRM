using System.Security.Claims;
using AcxiomCrm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCrm.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teamAccess = User.IsInRole("Admin") || User.IsInRole("Manager");
        var model = await _dashboard.BuildAsync(userId, teamAccess);
        return View(model);
    }
}
