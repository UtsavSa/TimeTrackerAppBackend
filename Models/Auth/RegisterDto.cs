
// Models/Auth/RegisterDto.cs

using System.ComponentModel.DataAnnotations;

namespace TimeTrackerApi.Models
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        // min 8, mist include a digit, and a non aplhanumeric symbol. 

        [RegularExpression(@"^(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Password must be 8+ chars and include a number and a symbol.")]
        public string Password { get; set; } = string.Empty;    
    }
}
