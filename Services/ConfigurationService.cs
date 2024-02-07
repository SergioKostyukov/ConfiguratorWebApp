using ConfiguratorWebApp.Data;
using ConfiguratorWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        LogLine(clean: true);

        var json = File.ReadAllText(configJsonFilePath);

        var configuration = ParseJsonRecursive(json, null);

        SaveConfiguration(configuration);
    }

    // Method to load configuration from TXT file
    public void LoadConfigurationFromTxt()
    {
        LogLine(clean: true);

        var lines = File.ReadAllLines(configTxtFilePath);

        var configuration = ParseTxt(lines);

        SaveConfiguration(configuration);
    }

    // Method to get configurations from the database
    public List<Configuration> GetConfiguration(string path)
    {
        // If the path is empty or null, we return the entire configuration
        if (string.IsNullOrEmpty(path))
        {
            return [.. _dbContext.Configurations];
        }

        // Find the new root element
        Guid? parentId = null;

        var segments = path.Split('/');
        foreach (var segment in segments)
        {
            parentId = FindConfigurationPartRoot(segment, parentId);
        }

        // Find part of tree
        List<Configuration> configurations = [];

        FindConfigurationPartRecursive(configurations, parentId);

        return configurations;
    }


    /* ------------------------- GetConfiduraton methods ------------------------- */
    // Find element root id by path value
    private Guid? FindConfigurationPartRoot(string segment, Guid? parentId)
    {
        var parent = _dbContext.Configurations
            .FirstOrDefault(c => c.Key == segment && c.ParentId == parentId);

        return parent?.Id;
    }

    // Find part of tree by path value
    private void FindConfigurationPartRecursive(List<Configuration> configurations, Guid? parentId, bool isFirstCall = true)
    {
        var root = _dbContext.Configurations
        .Where(c => c.ParentId == parentId)
        .ToList();

        // Checking if it`s a leaf element
        if (root.Count == 0)
        {
            var element = _dbContext.Configurations.FirstOrDefault(conf => conf.Id == parentId);
            if (element != null)
            {
                LogLine($"Final element\n{element.Id}, {element.ParentId}, {element.Key}, {element.Value}");
                element.ParentId = null;
                configurations.Add(element);
            }

            return;
        }

        // If it`s a branch element
        foreach (var conf in root)
        {
            LogLine($"{conf.Id}, {conf.ParentId}, {conf.Key}, {conf.Value}");

            if (isFirstCall)
            {
                conf.ParentId = null;
            }
            configurations.Add(conf);

            FindConfigurationPartRecursive(configurations, conf.Id, false);
        }
    }

    /* ------------------------- LoadConfiguration methods ------------------------- */
    // Parse JSON and build configuration objects
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

    // Parse configuration from TXT file
    private static List<Configuration> ParseTxt(string[] lines)
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

    // Parse configuration line from TXT file
    private static void ParseTxtLineRecursive(List<Configuration> configurations, string[] parts, Guid? parentId)
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
            // Checking if this branch already exists
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

    // Find element Guid in local storage
    private static Guid? FindConfiguration(List<Configuration> configurations, Guid? parentId, string key)
    {
        var element = configurations.FirstOrDefault(conf => conf.ParentId == parentId && conf.Key == key);

        return element?.Id;
    }

    // Method to save configurations to the database
    private void SaveConfiguration(List<Configuration> configuration)
    {
        // Remove existing configurations
        _dbContext.Configurations.RemoveRange(_dbContext.Configurations);

        // Add new configurations
        _dbContext.Configurations.AddRange(configuration);
        _dbContext.SaveChanges();
    }

    /* ------------------------- Logger methods ------------------------- */
    // Log information about a config item or a simple line to a log file
    private static void LogConfiguration(Configuration item)
    {
        using StreamWriter writer = new("./Data/log.txt", true);
        writer.WriteLine($"Id: {item.Id}, ParentId: {item.ParentId}, Key: {item.Key}, Value: {item.Value}");
    }
    private static void LogLine(string line = "", bool clean = false)
    {
        using StreamWriter writer = new("./Data/log.txt", !clean);
        writer.WriteLine(line);
    }
}
