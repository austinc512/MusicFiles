using System.ComponentModel.DataAnnotations;
using MusicFiles.Core.Enums;

namespace MusicFiles.Core.DTOs.Request
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name cannot be blank")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "Last name cannot be blank")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Username cannot be blank")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 20 characters long")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]*$", 
            ErrorMessage = "Username can only contain letters, numbers, underscores, hyphens, and periods")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Email cannot be blank")]
        [EmailAddress(ErrorMessage = "Email address should be in a proper format")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Phone number cannot be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should only contain numbers")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Password cannot be blank")]
        [DataType(DataType.Password)]
        // Password annotation can still provide semantic metadata (Swagger, if you use it),
        // but this provides no functionality
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$", 
            ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one non-alphanumeric character.")]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password cannot be blank")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = String.Empty;

        // Admin-level privileges must be set with another controller and DTO. 
        // public UserTypeOptions UserType { get; set; } = UserTypeOptions.User;
        
    }    
}