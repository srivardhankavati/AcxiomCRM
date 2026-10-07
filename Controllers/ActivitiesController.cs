using AcxiomCrm.Data;using AcxiomCrm.Models;using AcxiomCrm.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using System.Security.Claims;
namespace AcxiomCrm.Controllers;
[Authorize]public class ActivitiesController:Controller{private readonly ApplicationDbContext _db;private readonly AuditService _audit;public ActivitiesController(ApplicationDbContext db,AuditService audit){_db=db;_audit=audit;}
public async Task<IActionResult>Index(){var q=_db.Activities.AsNoTracking().AsQueryable();if(!User.IsInRole("Admin")&&!User.IsInRole("Manager")){var id=User.FindFirstValue(ClaimTypes.NameIdentifier);q=q.Where(x=>x.AssignedTo==id);}return View(await q.OrderByDescending(x=>x.ActivityDate).ToListAsync());}
public IActionResult Create()=>View(new Activity());
[HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(Activity x){if(!ModelState.IsValid)return View(x);x.AssignedTo=User.FindFirstValue(ClaimTypes.NameIdentifier);_db.Add(x);await _db.SaveChangesAsync();await _audit.LogAsync("Create","Activity",x.ActivityId.ToString(),newValue:x);return RedirectToAction(nameof(Index));}}
