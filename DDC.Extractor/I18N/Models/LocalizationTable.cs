using System.Collections.Generic;

namespace DDC.Extractor.I18N.Models;

public class LocalizationTable
{
    public string LanguageCode { get; init; } = "";
    public Dictionary<int, string> Entries { get; init; } = new();
}
