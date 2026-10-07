using System.ComponentModel.DataAnnotations;

namespace AcxiomCrm.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
public class RegisterViewModel
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(8)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; } = "";
    [Required] public string Role { get; set; } = "SalesExecutive";
}
