using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;

using OpeniddictServer.Data;

namespace OpeniddictServer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IOpenIddictApplicationManager _openIddictApplicationManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IOpenIddictApplicationManager openIddictApplicationManager)
    {
        _logger = logger;
        _dbContext = dbContext;
        _openIddictApplicationManager = openIddictApplicationManager;
    }

    public class Client
    {
        public string ClientId { get; set; }

        public string DisplayName { get; set; }

        public List<string> RedirectUris { get; set; }

        public List<string> Permissions { get; set; }
    }

    public async Task<IActionResult> Index()
    {
        var clientList = new List<Client>();

        await foreach (var client in _openIddictApplicationManager.ListAsync())
        {
            clientList.Add(new Client
            {
                ClientId = await _openIddictApplicationManager.GetClientIdAsync(client),
                DisplayName = await _openIddictApplicationManager.GetDisplayNameAsync(client),
                RedirectUris = [.. (await _openIddictApplicationManager.GetRedirectUrisAsync(client))],
                Permissions = [.. (await _openIddictApplicationManager.GetPermissionsAsync(client))],
            });
        }

        return View(clientList);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
