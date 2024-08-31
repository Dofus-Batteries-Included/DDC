using DDC.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DDC.Api.Controllers;

/// <summary>
///     Retrieve the available versions.
/// </summary>
[Route("/versions")]
[ApiController]
public class VersionsController : ControllerBase
{
    readonly IRawDataRepository _repository;

    /// <summary>
    /// </summary>
    public VersionsController(IRawDataRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Get available versions
    /// </summary>
    [HttpGet]
    public async Task<IReadOnlyCollection<string>> GetAvailableVersions() => await _repository.GetAvailableVersionsAsync();
}
