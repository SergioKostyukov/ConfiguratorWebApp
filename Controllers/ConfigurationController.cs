using ConfiguratorWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfiguratorWebApp.Controllers;

public class ConfigurationController(ConfigurationService configService) : Controller
{
    private readonly ConfigurationService _configService = configService;

    public IActionResult GetConfigurationTree()
    {
        var configuration = _configService.LoadConfigurationFromJson();

        return View(configuration);
    }
}
