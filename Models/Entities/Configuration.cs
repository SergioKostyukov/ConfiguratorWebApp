namespace ConfiguratorWebApp.Models.Entities;

public class Configuration
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Key { get; set; }
    public string? Value { get; set; }
}
