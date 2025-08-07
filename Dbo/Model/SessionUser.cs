namespace RetailAppS.Dbo.Model
{
    public class SessionUser
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public long? SessionId { get; set; }
        public Session? Session { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime ExpiresAt { get; set; }
    }
}