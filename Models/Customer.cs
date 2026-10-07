using System.ComponentModel.DataAnnotations;

namespace AcxiomCrm.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = "";
    [Required, StringLength(100)] public string CustomerName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(20)] public string Phone { get; set; } = "";
    [StringLength(120)] public string? CompanyName { get; set; }
    [StringLength(250)] public string? Address { get; set; }
    [StringLength(80)] public string? City { get; set; }
    [StringLength(80)] public string? State { get; set; }
    [Required] public string Status { get; set; } = "Active";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
