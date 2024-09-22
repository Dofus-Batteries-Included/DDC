using System.Collections.Generic;
using UnityBundleReader.Classes;

namespace DDC.Extractor.BundleExtractors;

public interface IBundleExtractor<out TData>
{
    TData Extract(IReadOnlyCollection<MonoBehaviour> behaviours);
}
