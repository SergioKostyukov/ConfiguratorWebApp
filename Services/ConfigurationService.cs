using ConfiguratorWebApp.Models;
using Newtonsoft.Json;
using System.IO;

namespace ConfiguratorWebApp.Services;

public class ConfigurationService
{
    public ConfigurationModel LoadConfigurationFromJson(string filePath)
    {
        string json = File.ReadAllText(filePath);

        ConfigurationModel configuration = JsonConvert.DeserializeObject<ConfigurationModel>(json);

        // Логування зчитаних даних
        string outputFilePath = "./Data/log.txt";
        SaveConfigurationToLog(configuration, outputFilePath);

        return configuration;
    }

    public void SaveConfigurationToLog(ConfigurationModel configuration, string filePath)
    {
        string json = JsonConvert.SerializeObject(configuration, Formatting.Indented);

        File.WriteAllText(filePath, json);
    }
}
