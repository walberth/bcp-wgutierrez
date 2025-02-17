namespace API_ORDER.Configuration
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
        public string GroupId { get; set; }
    }
}
