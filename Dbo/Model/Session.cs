namespace Ondrej.Dbo.Model
{
    [System.Serializable]
    public class Session
    {
        public long Id { get; set; }
        public string SessionId { get; set; } 
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public System.DateTime AccessedAt { get; set; }
    }
}
