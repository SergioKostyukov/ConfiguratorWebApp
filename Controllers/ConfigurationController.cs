using ConfiguratorWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConfiguratorWebApp.Controllers;

public class ConfigurationController(ConfigurationService configService) : Controller
{
    private readonly ConfigurationService _configService = configService;

    public IActionResult GetConfigurationTree()
    {
        _configService.LoadConfigurationFromJson();
        //_configService.LoadConfigurationFromTxt();

        var configuration = _configService.GetConfiguration();

        return View(configuration);
    }
}
