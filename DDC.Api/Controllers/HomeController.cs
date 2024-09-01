using Microsoft.AspNetCore.Mvc;

namespace DDC.Api.Controllers;

/// <summary>
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// </summary>
    public IActionResult Index() => View();
}
