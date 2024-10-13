using System.Collections.Generic;
using System.Linq;

namespace DDC.Extractor.Extensions;

public static class CollectionExtensions
{
    public static List<T> ToCSharpList<T>(this Il2CppSystem.Collections.Generic.List<T> list) => list._items.Take(list.Count).ToList();
    public static IReadOnlyCollection<T> ToNonEmptyCSharpListOrNull<T>(this Il2CppSystem.Collections.Generic.List<T> data) => data.Count == 0 ? null : data.ToCSharpList();
}
