#pragma warning disable 8632

namespace RetailAppS.Dbo.Model
{
    [System.Serializable]
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public string? EmailVerificationCode { get; set; }
        public bool? IsEmailVerified { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
    }
}
