using System.Collections.Generic;
using System.Linq;
using Core.DataCenter;

namespace DDC.Extractor.Converters;

public interface IConverter<in TData, out TConvertedData>
{
    TConvertedData Convert(TData data);
}

public static class ConverterExtensions
{
    public static IReadOnlyCollection<TConvertedData> ExtractDataFrom<TData, TConvertedData>(this IConverter<TData, TConvertedData> converter, MetadataRoot<TData> root)
    {
        Il2CppSystem.Collections.Generic.List<TData> data = root.GetObjects();
        return data._items.Take(data.Count).Select(converter.Convert).ToArray();
    }
}
