using System.ComponentModel.DataAnnotations;
using MusicFiles.Core.Enums;

namespace MusicFiles.Core.DTOs.Request
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name cannot be blank")]
        public string? PersonName { get; set; } 
        [Required(ErrorMessage = "Email cannot be blank")]
        [EmailAddress(ErrorMessage = "Email address should be in a proper format")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Phone cannot be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should only contain numbers")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Password cannot be blank")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one digit, and one non-alphanumeric character.")]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password cannot be blank")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = String.Empty;

        public UserTypeOptions UserType { get; set; } = UserTypeOptions.User;
    }    
}