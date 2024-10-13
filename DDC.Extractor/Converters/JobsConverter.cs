using Core.DataCenter.Metadata.Job;
using DDC.Extractor.Models.Jobs;

namespace DDC.Extractor.Converters;

public class JobsConverter : IConverter<Jobs, Job>
{
    public Job Convert(Jobs data) => new(data);
}
