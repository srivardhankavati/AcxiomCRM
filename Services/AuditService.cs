using System.Security.Claims;
using System.Text.Json;
using AcxiomCrm.Data;
using AcxiomCrm.Models;
using Microsoft.AspNetCore.Http;

namespace AcxiomCrm.Services;

public sealed class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    public async Task WriteAsync(
        string action,
        string entityName,
        string? recordId = null,
        object? before = null,
        object? after = null,
        string outcome = "Success",
        string? details = null)
    {
        var http = _httpContext.HttpContext;
        var entry = new AuditLog
        {
            UserId = http?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            Action = action,
            EntityName = entityName,
            RecordId = recordId,
            OldValue = before is null ? null : JsonSerializer.Serialize(before),
            NewValue = after is null ? null : JsonSerializer.Serialize(after),
            CreatedDate = DateTime.UtcNow,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            Result = outcome,
            Details = details
        };

        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync();
    }

    // Kept as a small compatibility wrapper for the existing controllers.
    public Task LogAsync(string action, string entity, string? recordId = null,
        object? oldValue = null, object? newValue = null,
        string result = "Success", string? details = null)
        => WriteAsync(action, entity, recordId, oldValue, newValue, result, details);
}
