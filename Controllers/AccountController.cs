using AcxiomCrm.Models;
using AcxiomCrm.Services;
using AcxiomCrm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCrm.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;
    public AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, AuditService audit) { _signIn = signIn; _users = users; _audit = audit; }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _users.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive()) { ModelState.AddModelError("", "Invalid credentials or inactive account."); await _audit.LogAsync("Login", "Authentication", result: "Failure"); return View(model); }
        var result = await _signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        await _audit.LogAsync(result.Succeeded ? "Login" : "Failed Login", "Authentication", user.Id, result: result.Succeeded ? "Success" : "Failure", details: result.IsLockedOut ? "Account locked" : null);
        if (result.Succeeded) return LocalRedirect(returnUrl ?? "/Dashboard");
        ModelState.AddModelError("", result.IsLockedOut ? "Account is temporarily locked." : "Invalid email or password.");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!User.IsInRole("Admin")) model.Role = "SalesExecutive";
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View(model); }
        await _users.AddToRoleAsync(user, model.Role);
        await _audit.LogAsync("Create", "User", user.Id, newValue: new { user.Email, model.Role });
        await _signIn.SignInAsync(user, false);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction("Login"); }
    public IActionResult AccessDenied() => View();
}

internal static class UserExtensions
{
    public static bool IsActive(this ApplicationUser user) => user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
}
