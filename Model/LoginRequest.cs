using System.ComponentModel.DataAnnotations;

namespace RetailAppS.Model
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
