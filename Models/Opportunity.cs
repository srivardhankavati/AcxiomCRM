using System.ComponentModel.DataAnnotations;

namespace AcxiomCrm.Models;

public class Opportunity
{
    public int OpportunityId { get; set; }
    [Required, StringLength(120)] public string OpportunityName { get; set; } = "";
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    [Range(typeof(decimal), "0.01", "1000000000000")] public decimal Amount { get; set; }
    [Required] public string Stage { get; set; } = "Qualification";
    [Range(0, 100)] public int Probability { get; set; }
    [DataType(DataType.Date)] public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(7);
    [Required] public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    public decimal WeightedValue => Amount * Probability / 100m;
}
