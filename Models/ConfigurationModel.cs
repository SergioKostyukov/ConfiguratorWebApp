namespace ConfiguratorWebApp.Models;

public class ConfigurationModel
{
    public string Key { get; set; }
    public string? Value { get; set; }
    public List<ConfigurationModel> Children { get; set; }

    public ConfigurationModel()
    {
        // Ініціалізуємо список Children як новий порожній список
        Children = new List<ConfigurationModel>();
    }
}
