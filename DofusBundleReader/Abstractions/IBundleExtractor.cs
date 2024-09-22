using UnityBundleReader.Classes;

namespace DofusBundleReader.Abstractions;

public interface IBundleExtractor<out TData>
{
    TData? Extract(IReadOnlyList<MonoBehaviour> behaviours);
}
