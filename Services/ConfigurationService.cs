using ConfiguratorWebApp.Data;
using ConfiguratorWebApp.Models.Entities;
using Newtonsoft.Json;

namespace ConfiguratorWebApp.Services;

public class ConfigurationService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private static readonly string configFilePath = "./ConfigExamples/config1.json";

    // Method to load configuration from JSON file
    public List<Configuration> LoadConfigurationFromJson()
    {
        var json = File.ReadAllText(configFilePath);
        var configuration = ParseJsonRecursive(json, null);
        SaveConfiguration(configuration);

        return configuration;
    }

    // Recursive method to parse JSON and build configuration objects
    private List<Configuration> ParseJsonRecursive(string json, Guid? parentId)
    {
        var Configurations = new List<Configuration>();
        dynamic jsonObject = JsonConvert.DeserializeObject(json);

        foreach (var property in jsonObject)
        {
            var item = new Configuration
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Key = property.Name,
                Value = property.Value is Newtonsoft.Json.Linq.JObject ? null : property.Value.ToString()
            };

            Configurations.Add(item); // Add configuration item

            LogConfiguration(item); // Log the configuration item

            if (property.Value is Newtonsoft.Json.Linq.JObject)
            {
                Configurations.AddRange(ParseJsonRecursive(property.Value.ToString(), item.Id));
            }
        }

        return Configurations;
    }

    // Method to save configurations to the database
    public void SaveConfiguration(List<Configuration> configuration)
    {
        // Remove existing configurations
        _dbContext.Configurations.RemoveRange(_dbContext.Configurations);

        // Add new configurations
        _dbContext.Configurations.AddRange(configuration);
        _dbContext.SaveChanges();
    }

    // Method to log information about a configuration item to a log file
    private static void LogConfiguration(Configuration item)
    {
        using StreamWriter writer = new("./Data/log.txt", true);
        writer.WriteLine($"Id: {item.Id}, ParentId: {item.ParentId}, Key: {item.Key}, Value: {item.Value}");
    }
}
