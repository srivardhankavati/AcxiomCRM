using System.ComponentModel.DataAnnotations;

namespace AcxiomCrm.Models;

public class Lead
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = "";
    [Required, StringLength(100)] public string LeadName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(20)] public string Phone { get; set; } = "";
    [StringLength(120)] public string? CompanyName { get; set; }
    [Required, StringLength(50)] public string Source { get; set; } = "Website";
    [Required] public string Status { get; set; } = "New";
    [Required] public string Priority { get; set; } = "Medium";
    [Range(0, 1000000000)] public decimal ExpectedValue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}
