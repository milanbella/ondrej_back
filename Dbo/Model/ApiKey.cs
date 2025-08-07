#pragma warning disable 8632
namespace Ondrej.Dbo.Model
{
    public class ApiKey
    {
        public int Id { get; set; }
        public string? KeyValue { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime ExpiresAt { get; set; }
    }
}
