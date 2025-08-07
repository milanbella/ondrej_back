using System.ComponentModel.DataAnnotations;

namespace Ondrej.Model
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
