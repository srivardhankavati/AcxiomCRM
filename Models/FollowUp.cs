using System.ComponentModel.DataAnnotations;

namespace AcxiomCrm.Models;

public class FollowUp
{
    public int FollowUpId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    [DataType(DataType.Date)] public DateTime FollowUpDate { get; set; } = DateTime.Today;
    [Required] public string FollowUpType { get; set; } = "Call";
    [Required, StringLength(500)] public string Remarks { get; set; } = "";
    [Required] public string Status { get; set; } = "Planned";
    public string? AssignedTo { get; set; }
}
