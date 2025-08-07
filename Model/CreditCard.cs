using System.ComponentModel.DataAnnotations;

namespace Ondrej.Model
{
    /// <summary>
    /// The annotaions are required for Validator.TryValidateObject during the user input of the data.
    /// </summary>
    public class CreditCard
    {
        [Required]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        [MinLength(13, ErrorMessage = "Card number must be at least 13 digits.")]
        [MaxLength(19, ErrorMessage = "Card number must be no more than 19 digits.")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Card number must contain only digits.")]
        public string? CardNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Expiration must be in MM/YY format.")]
        public string ExpirationDate { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string CVV { get; set; }
    }

}
