namespace ConfiguratorWebApp.Models.Entites
{
    public class Configuration
    {
        public Guid Id { get; set; }
        public int? PerentId { get; set; }
        public string Key { get; set; }
        public string? Value { get; set; }
    }
}
