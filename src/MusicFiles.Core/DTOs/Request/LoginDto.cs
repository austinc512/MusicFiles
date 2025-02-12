using System.ComponentModel.DataAnnotations;

namespace MusicFiles.Core.DTOs.Request;
/// <summary>
/// DTO for login requests, containing email and password.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email/Username can't be blank")]
    [StringLength(50, ErrorMessage = "Email/Username can't be longer than 50 characters")]
    public string UserNameOrEmail { get; set; } = String.Empty;
    [Required(ErrorMessage = "Password can't be blank")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = String.Empty;
    public bool RememberMe { get; set; } = false;
}