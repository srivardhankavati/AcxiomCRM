using AcxiomCrm.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCrm.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        builder.Entity<Customer>().HasIndex(x => x.Phone).IsUnique();
        builder.Entity<Customer>().HasIndex(x => x.CustomerCode).IsUnique();
        builder.Entity<Lead>().HasIndex(x => x.LeadCode).IsUnique();
        builder.Entity<Opportunity>().Property(x => x.Amount).HasPrecision(18, 2);
        builder.Entity<Lead>().Property(x => x.ExpectedValue).HasPrecision(18, 2);
        builder.Entity<Customer>().Property(x => x.Status).HasMaxLength(30);
        builder.Entity<Lead>().Property(x => x.Status).HasMaxLength(30);
        builder.Entity<Opportunity>().Property(x => x.Stage).HasMaxLength(30);
        builder.Entity<FollowUp>().Property(x => x.Status).HasMaxLength(30);
    }
}
