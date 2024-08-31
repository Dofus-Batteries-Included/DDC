using DDC.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DDC.Api.Controllers;

/// <summary>
///     Retrieve raw data in JSON files.
/// </summary>
[Route("{version}/raw")]
[ApiController]
public class RawDataController : ControllerBase
{
    readonly IRawDataRepository _repository;

    /// <summary>
    /// </summary>
    public RawDataController(IRawDataRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Get localization data
    /// </summary>
    /// <remarks>
    ///     Returns a JSON file.
    /// </remarks>
    [HttpGet("i18n/{lang}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<FileResult> GetI18N(string version, Language lang)
    {
        RawDataType type = lang switch
        {
            Language.Fr => RawDataType.I18NFr,
            Language.En => RawDataType.I18NEn,
            Language.Es => RawDataType.I18NEs,
            Language.De => RawDataType.I18NDe,
            Language.Pt => RawDataType.I18NPt,
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null)
        };

        IRawDataFile file = await _repository.GetRawDataFileAsync(version, type);
        return File(file.OpenRead(), file.ContentType, file.Name);
    }

    /// <summary>
    ///     Get map positions data
    /// </summary>
    /// <remarks>
    ///     Returns a JSON file.
    /// </remarks>
    [HttpGet("map-positions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<FileResult> GetMapPositions(string version)
    {
        IRawDataFile file = await _repository.GetRawDataFileAsync(version, RawDataType.MapPositions);
        return File(file.OpenRead(), file.ContentType, file.Name);
    }

    /// <summary>
    ///     Get points of interest data
    /// </summary>
    /// <remarks>
    ///     Returns a JSON file.
    /// </remarks>
    [HttpGet("points-of-interest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<FileResult> GetPointsOfInterest(string version)
    {
        IRawDataFile file = await _repository.GetRawDataFileAsync(version, RawDataType.PointOfInterest);
        return File(file.OpenRead(), file.ContentType, file.Name);
    }
}
