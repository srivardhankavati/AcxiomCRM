using AcxiomCrm.Models;
using Microsoft.AspNetCore.Identity;

namespace AcxiomCrm.Data;

public static class SeedData
{
    private static readonly string[] RoleNames = { "Admin", "Manager", "SalesExecutive" };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in RoleNames)
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var roleResult = await roles.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        await EnsureAccount(users, "admin@acxiomcrm.local", "CRM Administrator", "Admin@123", "Admin");
        await EnsureAccount(users, "manager@acxiomcrm.local", "Sales Manager", "Manager@123", "Manager");
        await EnsureAccount(users, "sales@acxiomcrm.local", "Sales Executive", "Sales@123", "SalesExecutive");
    }

    private static async Task EnsureAccount(
        UserManager<ApplicationUser> users,
        string email,
        string fullName,
        string password,
        string role)
    {
        var account = await users.FindByEmailAsync(email);

        if (account is null)
        {
            account = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var created = await users.CreateAsync(account, password);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
        }

        if (!await users.IsInRoleAsync(account, role))
            await users.AddToRoleAsync(account, role);
    }
}
