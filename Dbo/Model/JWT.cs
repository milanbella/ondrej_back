#pragma warning disable 8632
namespace Ondrej.Dbo.Model
{
    [System.Serializable]
    public class JWT
    {
        public long Id { get; set; }
        public string? Token { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        // Do not remove unused System namespace or else it will not compile in Gao  
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime ExpiresAt { get; set; }

        public long? DeviceId { get; set; }
        public Device? Device { get; set; }
    }
}
