using System.ComponentModel.DataAnnotations;

namespace MusicFiles.Core.DTOs.Request;
/// <summary>
/// DTO for login requests, containing email and password.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
    [DataType(DataType.EmailAddress)]
    public string UserEmail { get; set; } = String.Empty;
    [Required(ErrorMessage = "Password can't be blank")]
    [DataType(DataType.Password)]
    public string UserPassword { get; set; } = String.Empty;
    public bool UserRememberMe { get; set; } = false;
}