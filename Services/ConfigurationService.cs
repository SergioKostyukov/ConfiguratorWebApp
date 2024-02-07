using ConfiguratorWebApp.Data;
using ConfiguratorWebApp.Models.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConfiguratorWebApp.Services;

public class ConfigurationService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private static readonly string configJsonFilePath = "./ConfigExamples/config1.json";
    private static readonly string configTxtFilePath = "./ConfigExamples/config3.txt";

    // Method to load configuration from JSON file
    public void LoadConfigurationFromJson()
    {
        var json = File.ReadAllText(configJsonFilePath);

        var configuration = ParseJsonRecursive(json, null);

        SaveConfiguration(configuration);
    }

    // Method to load configuration from TXT file
    public void LoadConfigurationFromTxt()
    {
        var lines = File.ReadAllLines(configTxtFilePath);

        var configuration = ParseTxt(lines);

        SaveConfiguration(configuration);
    }

    public List<Configuration> GetConfiguration()
    {
        var configuration = _dbContext.Configurations.ToList();

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

    // Method to parse configuration from TXT file
    private List<Configuration> ParseTxt(string[] lines)
    {
        var configurations = new List<Configuration>();

        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length < 2)
            {
                continue;
            }
            else
            {
                ParseTxtLineRecursive(configurations, parts, null);
            }
        }

        return configurations;
    }

    // Method to parse configuration line from TXT file
    private void ParseTxtLineRecursive(List<Configuration> configurations, string[] parts, Guid? parentId)
    {
        if (parts.Length == 2)
        {
            var item = new Configuration
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Key = parts[0],
                Value = parts[1]
            };

            configurations.Add(item);
            LogConfiguration(item);
        }
        else
        {
            var id = FindConfiguration(configurations, parentId, parts[0]);
            if (id != null)
            {
                parentId = id;
            }
            else
            {
                var item = new Configuration
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId,
                    Key = parts[0],
                    Value = null
                };

                configurations.Add(item);
                LogConfiguration(item);

                parentId = item.Id;
            }

            ParseTxtLineRecursive(configurations, parts.Skip(1).ToArray(), parentId);
        }
    }

    // Method to find element Guid in local storage
    private Guid? FindConfiguration(List<Configuration> configurations, Guid? parentId, string key)
    {
        var element = configurations.FirstOrDefault(conf => conf.ParentId == parentId && conf.Key == key);
        if (element != null)
        {
            return element.Id;
        }
        else
        {
            return null;
        }
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
