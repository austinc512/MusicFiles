using System.ComponentModel.DataAnnotations;
using MusicFiles.Core.Enums;

namespace MusicFiles.Core.DTOs.Request
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name cannot be blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name cannot be blank.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Username cannot be blank.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 20 characters long.")]
        [RegularExpression(@"^(?!.*\.\.)(?!\.)[a-zA-Z0-9_.-]{5,20}(?<!\.)$", 
            ErrorMessage = "Username can only contain letters, numbers, underscores, hyphens, and periods. " +
                           "Periods cannot be consecutive, at the start, or at the end.")]

        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email cannot be blank.")]
        [EmailAddress(ErrorMessage = "Email address should be in a proper format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number cannot be blank.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should only contain numbers.")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password cannot be blank.")]
        [DataType(DataType.Password)]
        // Password annotation can still provide semantic metadata (Swagger, if you use it),
        // but this provides no functionality
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{6,50}$",
            ErrorMessage =
                "Password must be between 6 and 50 characters long, contain at least one uppercase letter, " +
                "one lowercase letter, one digit, and one non-alphanumeric character.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password cannot be blank.")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; } = String.Empty;

        [Required(ErrorMessage = "User type must be set.")]
        public UserTypeOptions UserType { get; set; } = UserTypeOptions.Customer;
        // This could be used if Authentication flow is set to log in automatically after registration.
        // However, certain actions should only be performed after confirming email address.
        // public bool RememberMe { get; set; } = false;
    }
}