#pragma warning disable 8632
namespace Ondrej.Dbo.Model
{

    [System.Serializable]
    public class Device
    {
        public long Id { get; set; }
        public string? DeviceId { get; set; }
        public long? SessionId { get; set; }
        public Session? Session { get; set; }

        public System.DateTime RegisteredAt { get; set; }
    }
}
