#pragma warning disable CS8618, CS8632
namespace RetailAppS.Dbo.Model
{
    public class UserVerificationCode
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string Code { get; set; }
        public System.DateTime ExpiresAt { get; set; }
    }
}
