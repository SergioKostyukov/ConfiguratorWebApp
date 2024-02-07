using ConfiguratorWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfiguratorWebApp.Controllers;

public class ConfigurationController(ConfigurationService configService) : Controller
{
    private readonly ConfigurationService _configService = configService;

    public IActionResult GetConfigurationTree(string path)
    {
        _configService.LoadConfigurationFromJson();
        //_configService.LoadConfigurationFromTxt();

        var configuration = _configService.GetConfiguration(path);

        if (configuration == null)
        {
            return NotFound();
        }

        return View(configuration);
    }
}
