using UnityBundleReader.Classes;

namespace DofusBundleReader.Abstractions;

public interface IBundleExtractor<out TData>
{
    TData? Extract(IReadOnlyCollection<MonoBehaviour> behaviours);
}
