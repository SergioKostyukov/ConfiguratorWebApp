using Microsoft.AspNetCore.Mvc;
using ConfiguratorWebApp.Services;
using ConfiguratorWebApp.Models;

namespace ConfiguratorWebApp.Controllers;

public class ConfigurationController : Controller
{
    private readonly ConfigurationService _configManager;

    public ConfigurationController(ConfigurationService configManager)
    {
        _configManager = configManager;
    }

    public IActionResult GetConfigurationTree()
    {
        var filePath = "./ConfigExamples/config2.json";
        var configuration = _configManager.LoadConfigurationFromJson(filePath);

        return View(configuration);
    }
}
