using ConfiguratorWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfiguratorWebApp.Controllers;

public class ConfigurationController(ConfigurationService configService, IConfiguration configuration) : Controller
{
    private readonly ConfigurationService _configService = configService;
    private readonly string _configFilePath = configuration["ConfigurationFilePath"];
    private readonly string _outputFilePath = "./Data/output.json";

	public IActionResult GetConfigurationTree(string path)
    {
        try
        {
            _configService.LoadConfiguration(_configFilePath);

            var configuration = _configService.GetConfiguration(path);

            _configService.LoadConfigurationToFile(_outputFilePath);

            if (configuration == null)
            {
                return NotFound();
            }

            return View(configuration);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing request: {ex.Message}");
        }
    }
}
