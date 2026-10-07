using AcxiomCrm.Data;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace AcxiomCrm.Controllers.Api;
[ApiController,Route("api/reports"),Authorize(Roles="Admin,Manager")]
public class ReportsApiController:ControllerBase{private readonly ApplicationDbContext _db;public ReportsApiController(ApplicationDbContext db)=>_db=db;[HttpGet("pipeline")]public async Task<IActionResult>Pipeline(){var data=await _db.Opportunities.GroupBy(x=>x.Stage).Select(g=>new{Stage=g.Key,Amount=g.Sum(x=>x.Amount),Weighted=g.Sum(x=>x.Amount*x.Probability/100m)}).ToListAsync();return Ok(data);}}
